using System;
using System.Collections.Generic;

namespace TallerPOO_Parte04
{
    // =========================================================================
    // 1. INTERFACES (Págs. 38-47) + PRINCIPIO DE SEGREGACIÓN DE INTERFACES (I)
    // =========================================================================
    public interface IMantenible
    {
        void RealizarMantenimiento();
    }

    public interface IRastreable
    {
        string ObtenerTelemetria();
    }

    // =========================================================================
    // 2. CLASE ABSTRACTA (Págs. 33-37) + ENCAPSULACIÓN Y CONSTRUCTORES (Págs. 1-28)
    // =========================================================================
    public abstract class Bicicleta : IMantenible
    {
        // Encapsulación: campos privados con validación en propiedades
        private string _marca = "Genérica";
        private string _modelo = "Estándar";
        private double _kilometraje;

        // Propiedad estática compartida por todas las instancias (Pág. 16)
        public static int TotalBicicletasRegistradas { get; private set; } = 0;

        public string Marca
        {
            get => _marca;
            protected set => _marca = string.IsNullOrWhiteSpace(value) ? "Genérica" : value;
        }

        public string Modelo
        {
            get => _modelo;
            protected set => _modelo = string.IsNullOrWhiteSpace(value) ? "Estándar" : value;
        }

        public double Kilometraje
        {
            get => _kilometraje;
            private set
            {
                if (value >= 0) _kilometraje = value;
            }
        }

        // Constructor parametrizado (Págs. 7-11)
        protected Bicicleta(string marca, string modelo, double kmIniciales)
        {
            Marca = marca;
            Modelo = modelo;
            Kilometraje = kmIniciales;
            TotalBicicletasRegistradas++;
        }

        // Sobrecarga de constructor con encadenamiento : this(...) (Pág. 10)
        protected Bicicleta(string marca, string modelo) : this(marca, modelo, 0)
        {
        }

        public void RegistrarRodada(double distanciaKm)
        {
            if (distanciaKm > 0)
            {
                Kilometraje += distanciaKm;
                Console.WriteLine($"  [+] Rodada registrada en {Marca} {Modelo}: +{distanciaKm} km (Total: {Kilometraje} km)");
            }
        }

        // Método virtual: puede ser sobrescrito por las subclases (Polimorfismo - Pág. 28)
        public virtual void MostrarFichaTecnica()
        {
            Console.WriteLine($"Bicicleta: {Marca} {Modelo} | Odómetro: {Kilometraje} km");
        }

        // Método abstracto: obliga a cada subclase a definir su propio mantenimiento (Pág. 34)
        public abstract void RealizarMantenimiento();
    }

    // =========================================================================
    // 3. COMPOSICIÓN (Págs. 48-50): Una bicicleta de ruta tiene un ciclocomputador
    // =========================================================================
    public class CiclocomputadorLixada : IRastreable
    {
        public int CircunferenciaRuedaMm { get; }
        public string MedidaNeumatico { get; }

        public CiclocomputadorLixada(string medidaNeumatico, int circunferenciaMm)
        {
            MedidaNeumatico = medidaNeumatico;
            CircunferenciaRuedaMm = circunferenciaMm;
        }

        public string ObtenerTelemetria()
        {
            return $"Sensor Lixada calibrado a {CircunferenciaRuedaMm} mm ({MedidaNeumatico})";
        }
    }

    // =========================================================================
    // 4. HERENCIA Y POLIMORFISMO (Págs. 22-32)
    // =========================================================================
    public class BicicletaRuta : Bicicleta
    {
        public string GrupoTransmision { get; }
        // Composición fuerte: el ciclocomputador forma parte de esta bicicleta
        public CiclocomputadorLixada Computador { get; }

        // Uso de : base(...) para llamar al constructor de la clase padre (Pág. 24)
        public BicicletaRuta(string marca, string modelo, string grupo, int circunferenciaMm)
            : base(marca, modelo)
        {
            GrupoTransmision = grupo;
            Computador = new CiclocomputadorLixada("700x25c", circunferenciaMm);
        }

        // Sobrescritura polimórfica (override)
        public override void MostrarFichaTecnica()
        {
            Console.WriteLine($"[RUTA] {Marca} {Modelo} | Grupo: {GrupoTransmision} | {Computador.ObtenerTelemetria()} | Odómetro: {Kilometraje} km");
        }

        public override void RealizarMantenimiento()
        {
            Console.WriteLine($"  -> Mantenimiento Ruta ({Marca} {Modelo}): Indexado de cambios {GrupoTransmision}, alineación de pinzas de freno y lubricación de cadena.");
        }
    }

    // Clase sellada (sealed): no permite más herencia hacia abajo (Pág. 26)
    public sealed class BicicletaMontana : Bicicleta
    {
        public string TipoSuspension { get; }

        public BicicletaMontana(string marca, string modelo, string suspension)
            : base(marca, modelo)
        {
            TipoSuspension = suspension;
        }

        public override void MostrarFichaTecnica()
        {
            Console.WriteLine($"[MTB]  {Marca} {Modelo} | Suspensión: {TipoSuspension} | Odómetro: {Kilometraje} km");
        }

        public override void RealizarMantenimiento()
        {
            Console.WriteLine($"  -> Mantenimiento MTB ({Marca} {Modelo}): Limpieza de barras de horquilla ({TipoSuspension}), ajuste de tensión y engrase de cadena.");
        }
    }

    // =========================================================================
    // 5. PATRÓN STRATEGY + PRINCIPIO ABIERTO/CERRADO (O) (Págs. 54-62)
    // =========================================================================
    public interface IEstrategiaRuta
    {
        string NombrePlan { get; }
        double CalcularDistanciaEstimada(int minutosDisponibles);
    }

    public class RutaVelozAsfalto : IEstrategiaRuta
    {
        public string NombrePlan => "Entrenamiento de Velocidad en Asfalto";
        public double CalcularDistanciaEstimada(int minutosDisponibles) => Math.Round((minutosDisponibles / 60.0) * 31.5, 1);
    }

    public class RutaTecnicaMixta : IEstrategiaRuta
    {
        public string NombrePlan => "Rodada Técnica de Resistencia";
        public double CalcularDistanciaEstimada(int minutosDisponibles) => Math.Round((minutosDisponibles / 60.0) * 19.0, 1);
    }

    // =========================================================================
    // 6. PATRÓN SINGLETON (Págs. 51-53) + INVERSIÓN DE DEPENDENCIAS (D) (Pág. 65)
    // =========================================================================
    public class BitacoraTaller
    {
        private static BitacoraTaller? _instancia;
        private readonly List<string> _registros = new();

        // Constructor privado: impide hacer 'new BitacoraTaller()' desde afuera
        private BitacoraTaller() { }

        public static BitacoraTaller Instancia => _instancia ??= new BitacoraTaller();

        public void Anotar(string mensaje)
        {
            string entrada = $"[{DateTime.Now:HH:mm:ss}] {mensaje}";
            _registros.Add(entrada);
        }

        public void ImprimirHistorial()
        {
            Console.WriteLine("\n=== BITÁCORA ÚNICA DEL TALLER (Patrón Singleton) ===");
            foreach (var item in _registros)
            {
                Console.WriteLine(item);
            }
        }
    }

    // =========================================================================
    // 7. AGREGACIÓN (Pág. 49) + RESPONSABILIDAD ÚNICA (S) (Pág. 59)
    // =========================================================================
    public class Ciclista
    {
        public string Nombre { get; }
        public string Ciudad { get; }
        // Agregación: el ciclista tiene bicicletas, pero estas existen independientemente
        private readonly List<Bicicleta> _garaje = new();

        public Ciclista(string nombre, string ciudad)
        {
            Nombre = nombre;
            Ciudad = ciudad;
        }

        public void AgregarBicicleta(Bicicleta bici)
        {
            _garaje.Add(bici);
            BitacoraTaller.Instancia.Anotar($"{Nombre} agregó al garaje: {bici.Marca} {bici.Modelo}");
        }

        public void SalirARodar(Bicicleta bici, int minutos, IEstrategiaRuta estrategia)
        {
            Console.WriteLine($"\n> {Nombre} sale por {Ciudad} usando '{estrategia.NombrePlan}' ({minutos} min):");
            double km = estrategia.CalcularDistanciaEstimada(minutos);
            bici.RegistrarRodada(km);
            BitacoraTaller.Instancia.Anotar($"Rodada de {km} km en {bici.Marca} {bici.Modelo} ({estrategia.NombrePlan})");
        }

        public void RevisarGarajeCompleto()
        {
            Console.WriteLine($"\n=== GARAJE DE {Nombre.ToUpper()} ({Ciudad}) - Total registradas: {Bicicleta.TotalBicicletasRegistradas} ===");
            // Polimorfismo en acción: cada bici responde según su tipo real
            foreach (var bici in _garaje)
            {
                bici.MostrarFichaTecnica();
                bici.RealizarMantenimiento();
            }
        }
    }

    // =========================================================================
    // PROGRAMA PRINCIPAL
    // =========================================================================
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("==================================================================");
            Console.WriteLine("  SIMULADOR DE GARAJE Y TALLER CICLISTA - POO EN C# (PARTE 04)");
            Console.WriteLine("==================================================================");

            // Instanciación del ciclista
            var ciclista = new Ciclista("Kevin Hernández", "Santo Domingo");

            // Instanciación de objetos derivados (Herencia + Composición)
            var ruta = new BicicletaRuta("Specialized", "Allez 2021", "Shimano 105", 2105);
            var mtb = new BicicletaMontana("Julen", "K2", "Hardtail");

            // Agregación al garaje
            ciclista.AgregarBicicleta(ruta);
            ciclista.AgregarBicicleta(mtb);

            // Uso del patrón Strategy para simular salidas
            ciclista.SalirARodar(ruta, 60, new RutaVelozAsfalto());
            ciclista.SalirARodar(mtb, 45, new RutaTecnicaMixta());

            // Polimorfismo y ejecución de interfaces/métodos abstractos
            ciclista.RevisarGarajeCompleto();

            // Consulta de la instancia única (Singleton)
            BitacoraTaller.Instancia.ImprimirHistorial();

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
