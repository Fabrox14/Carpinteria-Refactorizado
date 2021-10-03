using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.dominio
{
    class Presupuesto
    {
        public int PresupuestoNro { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public DateTime FechaBaja { get; set; }
        public List<DetallePresupuesto> Detalles { get; }

        public Presupuesto()
        {
            // Generar la relacion 1 a muchos
            Detalles = new List<DetallePresupuesto>();
        }

        // Funcion para agregar los detalles del mismo presupuesto a una lista
        // pq un presupuesto tiene muchos detalles
        public void AgregarDetalle(DetallePresupuesto detalle)
        {
            Detalles.Add(detalle);
        }

        public void QuitarDetalle(int nro)
        {
            Detalles.RemoveAt(nro);
        }

        public double CalcularTotal()
        {
            double total = 0;

            foreach (DetallePresupuesto item in Detalles)
            {
                total += item.CalcularSubtotal();
            }

            return total;
        }

        //public bool Confirmar()
        //{
        //  paso al DAO
        //}

        //public bool Actualizar()
        //{
        //    paso al DAO            
        //}

        public double calcularTotalDesc(double total, double descuento)
        {
            return total - ((descuento * total) / 100);
        }

        public string GetFechaBajaFormato()
        {
            string aux = FechaBaja.ToString("dd/MM/yyyy");
            return aux.Equals("01/01/0001") ? "" : aux;
        }
    }
}
