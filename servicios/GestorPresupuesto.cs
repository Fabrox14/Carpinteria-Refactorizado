using Carpinteria_Refactorizado.accesoDatos;
using Carpinteria_Refactorizado.dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.servicios
{
    class GestorPresupuesto
    {
        private IPresupuestoDAO dao;

        public GestorPresupuesto(AbstractDAOFactory factory)
        {
            dao = factory.CrearPresupuestoDAO();
        }





        public int ProximoPresupuesto()
        {
            return dao.ObtenerProximoNroPresupuesto();
        }

        public DataTable ObtenerProductos()
        {
            return dao.ListarProductos();
        }

        public bool ConfirmarPresupuesto(Presupuesto oPresupuesto)
        {
            return dao.Crear(oPresupuesto);
        }

        public Presupuesto ObtenerPresupuestoPorID(int nro)
        {
            return dao.ObtenerPresupuestoPorID(nro);
        }

        public List<Presupuesto> ConsultarPresupuestos(List<Parametro> filtros)
        {
            return dao.ConsultarPresupuestos(filtros);
        }

        public bool RegistrarBajaPresupuesto(int idPresupuesto)
        {
            return dao.RegistrarBajaPresupuesto(idPresupuesto);
        }

        public bool ActualizarPresupuesto(Presupuesto oPresupuesto)
        {
            return dao.ActualizarPresupuesto(oPresupuesto);
        }
    }
}
