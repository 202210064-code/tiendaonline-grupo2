import http from 'k6/http';
import { sleep, check } from 'k6';

const URL = __ENV.URL;

export const options = {
  stages: [
    { duration: '1m', target: 50 },
    { duration: '1m', target: 200 },
    { duration: '2m30s', target: 200 },
    { duration: '30s', target: 0 },
  ],
};

export default function () {
  const r1 = http.get(`${URL}/`);
  check(r1, { 'portada 200': (r) => r.status === 200 });
  const r2 = http.get(`${URL}/?q=libro de programacion`);
  check(r2, { 'busqueda 200': (r) => r.status === 200 });
  sleep(1);
}