#!/usr/bin/env bash
# VPC Reachability Analyzer: comprueba por dónde PUEDE y por dónde NO PUEDE pasar el tráfico en la VPC de URPOnline.
# Ejecútalo en AWS CloudShell (o con la AWS CLI configurada):   bash scripts/analizar-red.sh
# Costo: Reachability Analyzer cobra ~0,10 USD por análisis (este script hace 4).
set -euo pipefail
STACK=${1:-URPOnlineLab}
REGION=${AWS_REGION:-us-east-2}

salida() { aws cloudformation describe-stacks --stack-name "$STACK" --region "$REGION" --query "Stacks[0].Outputs[?OutputKey=='$1'].OutputValue" --output text; }
VPC=$(salida VpcId)
IGW=$(aws ec2 describe-internet-gateways --region "$REGION" --filters Name=attachment.vpc-id,Values="$VPC" --query "InternetGateways[0].InternetGatewayId" --output text)
EC2=$(aws ec2 describe-instances --region "$REGION" --filters Name=vpc-id,Values="$VPC" Name=instance-state-name,Values=running --query "Reservations[0].Instances[0].InstanceId" --output text)
ALB_ENI=$(aws ec2 describe-network-interfaces --region "$REGION" --filters Name=vpc-id,Values="$VPC" Name=interface-type,Values=interface "Name=description,Values=ELB app/*" --query "NetworkInterfaces[0].NetworkInterfaceId" --output text)
RDS_ENI=$(aws ec2 describe-network-interfaces --region "$REGION" --filters Name=vpc-id,Values="$VPC" Name=description,Values=RDSNetworkInterface --query "NetworkInterfaces[0].NetworkInterfaceId" --output text)
echo "VPC=$VPC  IGW=$IGW  EC2=$EC2  ALB=$ALB_ENI  RDS=$RDS_ENI"
echo

analizar() { # nombre origen destino puerto esperado
  local path an estado ok
  path=$(aws ec2 create-network-insights-path --region "$REGION" --source "$2" --destination "$3" --protocol tcp --destination-port "$4" \
    --tag-specifications "ResourceType=network-insights-path,Tags=[{Key=Name,Value=urponline-$4}]" --query NetworkInsightsPath.NetworkInsightsPathId --output text)
  an=$(aws ec2 start-network-insights-analysis --region "$REGION" --network-insights-path-id "$path" --query NetworkInsightsAnalysis.NetworkInsightsAnalysisId --output text)
  estado=running
  while [ "$estado" = running ]; do sleep 5; estado=$(aws ec2 describe-network-insights-analyses --region "$REGION" --network-insights-analysis-ids "$an" --query "NetworkInsightsAnalyses[0].Status" --output text); done
  ok=$(aws ec2 describe-network-insights-analyses --region "$REGION" --network-insights-analysis-ids "$an" --query "NetworkInsightsAnalyses[0].NetworkPathFound" --output text)
  printf '%-45s %-14s (esperado: %s)\n' "$1" "$([ "$ok" = True ] && echo 'ALCANZABLE ✅' || echo 'BLOQUEADO 🔒')" "$5"
}

analizar "ALB -> EC2 : 80 (Nginx)"                  "$ALB_ENI" "$EC2"    80   "ALCANZABLE"
analizar "EC2 -> RDS : 5432 (PostgreSQL)"           "$EC2"     "$RDS_ENI" 5432 "ALCANZABLE"
analizar "Internet -> EC2 : 80 (saltando el ALB)"   "$IGW"     "$EC2"    80   "BLOQUEADO: subred privada"
analizar "Internet -> RDS : 5432"                   "$IGW"     "$RDS_ENI" 5432 "BLOQUEADO: subred de datos"
echo
echo "Detalle de cada análisis: consola de AWS -> VPC -> Reachability Analyzer."
