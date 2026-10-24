using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaOnline.Data.Migrations
{
    /// <inheritdoc />
    public partial class MasProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                INSERT INTO ""Products"" (""Name"", ""Category"", ""Description"", ""Price"", ""Stock"", ""Emoji"") VALUES
                -- Categoría 1: Herramientas
                ('Taladro Inalámbrico', 'Herramientas', 'Sirve para perforar paredes y madera sin usar cables. Facilita las tareas de reparación en el hogar.', 85.00, 15, '🛠️'),
                ('Set de Destornilladores', 'Herramientas', 'Utilizado para ajustar o retirar tornillos de diferentes tamaños. Ideal para ensamblar muebles.', 25.50, 40, '🪛'),
                ('Martillo de Carpintero', 'Herramientas', 'Diseñado para clavar y extraer clavos con facilidad. Fundamental en cualquier caja de herramientas.', 15.00, 30, '🔨'),
                ('Cinta Métrica Digital', 'Herramientas', 'Permite medir distancias con precisión milimétrica. Útil para calcular espacios antes de comprar muebles.', 30.00, 20, '📏'),
                ('Sierra Circular', 'Herramientas', 'Sirve para realizar cortes rectos y precisos en madera. Acelera los trabajos de carpintería.', 120.00, 10, '🪚'),
                ('Llave Inglesa Ajustable', 'Herramientas', 'Utilizada para aflojar o apretar tuercas de distintos diámetros. Evita comprar múltiples llaves fijas.', 18.50, 50, '🔧'),

                -- Categoría 2: Cocina
                ('Licuadora de Alta Potencia', 'Cocina', 'Sirve para triturar hielo y alimentos duros rápidamente. Perfecta para preparar batidos y sopas.', 95.00, 25, '🥤'),
                ('Sartén Antiadherente', 'Cocina', 'Evita que los alimentos se peguen al cocinar usando menos aceite. Facilita la limpieza posterior.', 35.00, 60, '🍳'),
                ('Juego de Cuchillos Chef', 'Cocina', 'Utilizado para cortar, rebanar y picar todo tipo de ingredientes. Mejora la velocidad y seguridad.', 60.00, 20, '🔪'),
                ('Tabla de Picar de Bambú', 'Cocina', 'Proporciona una superficie higiénica y resistente para cortar alimentos. Protege el filo de los cuchillos.', 20.00, 45, '🪓'),
                ('Balanza Digital de Cocina', 'Cocina', 'Sirve para medir el peso exacto de los ingredientes. Indispensable para recetas de repostería.', 15.00, 35, '⚖️'),
                ('Termómetro para Carnes', 'Cocina', 'Permite comprobar la temperatura interna de los alimentos. Garantiza que las carnes queden cocidas al punto.', 12.00, 15, '🌡️'),

                -- Categoría 3: Oficina
                ('Silla Ergonómica', 'Oficina', 'Proporciona soporte lumbar durante largas jornadas de trabajo. Ayuda a prevenir dolores de espalda.', 150.00, 12, '🪑'),
                ('Escritorio Ajustable', 'Oficina', 'Permite alternar entre trabajar sentado o de pie. Mejora la circulación y la postura del usuario.', 290.00, 5, '🖥️'),
                ('Lámpara LED de Escritorio', 'Oficina', 'Sirve para iluminar el área de trabajo sin cansar la vista. Incluye ajuste de intensidad.', 45.00, 30, '💡'),
                ('Soporte para Monitor', 'Oficina', 'Eleva la pantalla a la altura de los ojos. Evita la tensión en el cuello al mantener una postura correcta.', 35.00, 40, '💻'),
                ('Teclado Mecánico Silencioso', 'Oficina', 'Facilita la escritura rápida y precisa sin generar ruido molesto. Ideal para oficinas compartidas.', 80.00, 25, '⌨️'),
                ('Ratón Inalámbrico Vertical', 'Oficina', 'Mantiene la mano en una posición natural al navegar. Previene el síndrome del túnel carpiano.', 40.00, 30, '🖱️'),

                -- Categoría 4: Deportes
                ('Esterilla de Yoga', 'Deportes', 'Ofrece una superficie antideslizante y acolchada. Sirve para realizar ejercicios de suelo con comodidad.', 25.00, 50, '🧘'),
                ('Mancuernas Ajustables', 'Deportes', 'Permiten modificar el peso rápidamente según el ejercicio. Ahorran espacio al sustituir varias pesas.', 110.00, 15, '🏋️'),
                ('Cuerda para Saltar', 'Deportes', 'Utilizada para realizar ejercicios de cardio intensos en cualquier lugar. Ayuda a mejorar la resistencia.', 15.00, 60, '🪢'),
                ('Rodillo de Espuma', 'Deportes', 'Sirve para masajear los músculos después del entrenamiento. Acelera la recuperación y reduce la rigidez.', 22.00, 40, '🧻'),
                ('Banda de Resistencia', 'Deportes', 'Añade tensión a los movimientos para tonificar los músculos. Útil para rutinas de fuerza en casa.', 12.50, 70, '〰️'),
                ('Botella de Agua Térmica', 'Deportes', 'Mantiene las bebidas frías o calientes por horas. Esencial para la hidratación durante rutinas largas.', 28.00, 55, '💧'),

                -- Categoría 5: Electrónica
                ('Auriculares con Cancelación de Ruido', 'Electrónica', 'Bloquean los sonidos del entorno para mejorar la concentración. Sirven para trabajar en lugares ruidosos.', 199.00, 20, '🎧'),
                ('Batería Portátil', 'Electrónica', 'Proporciona energía adicional para recargar el móvil en movimiento. Evita que te quedes desconectado.', 45.00, 45, '🔋'),
                ('Altavoz Bluetooth', 'Electrónica', 'Permite reproducir música de forma inalámbrica desde cualquier dispositivo. Ideal para ambientar reuniones.', 55.00, 35, '🔊'),
                ('Cargador Inalámbrico Rápido', 'Electrónica', 'Sirve para recargar dispositivos compatibles con solo apoyarlos. Reduce el desgaste de los puertos.', 35.00, 50, '⚡'),
                ('Cable USB-C Reforzado', 'Electrónica', 'Utilizado para transferir datos y cargar baterías a alta velocidad. Su diseño trenzado evita roturas.', 15.00, 100, '🔌'),
                ('Lector de Tarjetas SD', 'Electrónica', 'Permite transferir fotos y videos desde una cámara al ordenador. Facilita la edición de archivos.', 20.00, 25, '💾');
            ";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Products"" WHERE ""Category"" IN ('Herramientas', 'Cocina', 'Oficina', 'Deportes', 'Electrónica');");
        }
    }
}