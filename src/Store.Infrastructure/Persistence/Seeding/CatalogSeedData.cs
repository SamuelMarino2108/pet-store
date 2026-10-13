namespace Store.Infrastructure.Persistence.Seeding;

/// <summary>
/// Sample catalog for the demo store: dog and cat accessories with realistic COP prices.
/// Images live in the web app under <c>/images/products/{sku}.webp</c>.
/// </summary>
internal static class CatalogSeedData
{
    public static IReadOnlyList<SeedCategory> Categories { get; } =
    [
        new("Camas y descanso", "BED",
        [
            new("Cama acolchada para perro mediano", "Relleno de fibra siliconada y funda removible lavable a máquina. Para perros de 10 a 20 kg.", 52_000, 109_900, 14),
            new("Cama ortopédica para perro grande", "Espuma viscoelástica que alivia articulaciones. Ideal para perros mayores o de más de 25 kg.", 98_000, 199_900, 6, 15),
            new("Cueva térmica para gato", "Cama tipo iglú con interior de felpa que conserva el calor. Base antideslizante.", 41_000, 84_900, 20),
            new("Cojín lavable antideslizante", "Cojín plano para guacal o sofá, con base de goma que evita que se deslice.", 28_000, 59_900, 0),
            new("Hamaca de ventana para gato", "Se fija al vidrio con ventosas y soporta hasta 15 kg. Para gatos que aman mirar afuera.", 35_000, 74_900, 9),
        ]),
        new("Juguetes", "TOY",
        [
            new("Pelota dispensadora de premios", "Libera premios mientras tu perro juega. Caucho natural resistente a mordidas.", 9_500, 24_900, 40),
            new("Cuerda de algodón para morder", "Cuerda trenzada que ayuda a limpiar los dientes. Ideal para juegos de tirar.", 7_800, 19_900, 55, 10),
            new("Varita con plumas para gato", "Varita flexible con plumas y cascabel para estimular el instinto de caza.", 6_200, 16_900, 60),
            new("Ratón de catnip (pack x3)", "Tres ratones de tela rellenos de catnip orgánico.", 8_400, 21_900, 35),
            new("Peluche resistente con chillido", "Peluche con doble costura y chillido interno. Para perros que juegan suave.", 12_000, 32_900, 22),
        ]),
        new("Collares y correas", "WLK",
        [
            new("Collar reflectivo ajustable", "Collar de nailon con franja reflectiva para paseos nocturnos. Talla ajustable.", 11_000, 29_900, 30),
            new("Correa retráctil de 5 metros", "Correa con freno de un botón y mango ergonómico. Para perros de hasta 25 kg.", 32_000, 69_900, 12, 20),
            new("Arnés acolchado antitirones", "Arnés con anilla frontal que reduce los tirones sin presionar el cuello.", 38_000, 84_900, 18),
            new("Collar con cascabel para gato", "Collar con cierre de seguridad que se suelta si el gato queda enganchado.", 5_500, 14_900, 45),
            new("Placa de identificación grabada", "Placa de acero inoxidable grabada con el nombre y teléfono de tu mascota.", 9_000, 22_900, 25),
        ]),
        new("Comederos y bebederos", "FED",
        [
            new("Fuente de agua con filtro para gato", "Agua en movimiento y filtrada que anima a tu gato a hidratarse. Capacidad 2 L.", 58_000, 129_900, 10),
            new("Comedero lento antiansiedad", "Laberinto que obliga a comer despacio y reduce el vómito y la hinchazón.", 14_500, 34_900, 28),
            new("Plato doble de acero inoxidable", "Dos platos con base de silicona antideslizante, aptos para lavavajillas.", 16_000, 39_900, 33),
            new("Dispensador automático de comida", "Programa hasta cuatro comidas al día con porciones exactas. Tolva de 4 L.", 115_000, 239_900, 4, 10),
            new("Bebedero portátil de viaje", "Botella con bandeja plegable para dar agua en los paseos. 500 ml.", 13_000, 29_900, 0),
        ]),
        new("Rascadores y gimnasios", "SCR",
        [
            new("Torre rascadora de 120 cm", "Tres niveles con postes de sisal, casita y plataforma superior para descansar.", 125_000, 259_900, 5),
            new("Rascador de cartón corrugado", "Rascador reversible de cartón reciclado con catnip incluido.", 15_000, 36_900, 26),
            new("Poste rascador de sisal", "Poste de 60 cm con base pesada que no se voltea.", 24_000, 54_900, 19, 15),
            new("Repisas de pared para gato (set x3)", "Tres repisas de madera para crear un circuito vertical en casa.", 68_000, 149_900, 7),
            new("Túnel plegable para gato", "Túnel de 90 cm con mirilla y juguete colgante. Se pliega para guardarlo.", 21_000, 47_900, 16),
        ]),
        new("Higiene y cuidado", "HYG",
        [
            new("Cepillo deslanador", "Retira el pelo muerto de la subcapa y reduce la caída hasta en un 90 %.", 13_500, 32_900, 38),
            new("Champú hipoalergénico 500 ml", "Fórmula suave con avena para pieles sensibles. Sin parabenos.", 12_000, 28_900, 42),
            new("Cortaúñas con protector", "Cortaúñas con tope de seguridad que evita cortar de más.", 8_000, 19_900, 50),
            new("Arena aglomerante sin aroma 10 kg", "Arena de bentonita que forma bloques firmes y controla los olores.", 26_000, 52_900, 30, 5),
            new("Bolsas biodegradables (pack x120)", "Ocho rollos de bolsas compostables con dispensador para la correa.", 9_000, 21_900, 64),
        ]),
    ];

    internal sealed record SeedCategory(string Name, string SkuPrefix, IReadOnlyList<SeedProduct> Products);

    internal sealed record SeedProduct(
        string Name,
        string Description,
        decimal Cost,
        decimal Price,
        int Stock,
        decimal? DiscountPercentage = null);
}
