using Carpinteria_Refactorizado.dominio;
using Carpinteria_Refactorizado.servicios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.accesoDatos
{
    class PresupuestoDAO : IPresupuestoDAO
    {
        // La interfaz asociada me obliga a implementar estos metodos
        public int ObtenerProximoNroPresupuesto()
        {
            return HelperDAO.ObtenerInstancia().ProximoID("SP_PROXIMO_ID", "@next");
        }

        public DataTable ListarProductos()
        {
            return HelperDAO.ObtenerInstancia().ConsultaSQL("SP_CONSULTAR_PRODUCTOS");
        }

        public bool Crear(Presupuesto oPresupuesto)
        {
            /*
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("@presupuesto_nro", 18);
            parametros.Add("@detalle_nro", 5);
            parametros.Add("@id_producto", 1);
            parametros.Add("@cantidad", 5);
            HelperDAO.ObtenerInstancia().EjecutarSQL("SP_INSERTAR_DETALLE", parametros);
            */

            return HelperDAO.ObtenerInstancia().Save(oPresupuesto);
        }

        public Presupuesto ObtenerPresupuestoPorID(int id)
        {
            return HelperDAO.ObtenerInstancia().GetById(id);
        }

        public List<Presupuesto> ConsultarPresupuestos(List<Parametro> criterios)
        {
            return HelperDAO.ObtenerInstancia().GetByFilters(criterios);
        }

        public bool RegistrarBajaPresupuesto(int idPresupuesto)
        {
            return HelperDAO.ObtenerInstancia().Delete(idPresupuesto);
        }

        public bool ActualizarPresupuesto(Presupuesto oPresupuesto)
        {
            return HelperDAO.ObtenerInstancia().Update(oPresupuesto);
        }
    }
}
