using Carpinteria_Refactorizado.dominio;
using System.Data;

namespace Carpinteria_Refactorizado.accesoDatos
{
    interface IPresupuestoDAO
    {
        // Todo lo que va a hacer contra la Base de Datos la clase de PresupuestoDAO
        
        int ObtenerProximoNroPresupuesto(); // metodo que estaba en Frm_Alta_Presupuesto.cs
        DataTable ListarProductos(); // metodo que estaba en Frm_Alta_Presupuesto.cs
        bool Crear(Presupuesto oPresupuesto); // metodo que estaba en Presupuesto.cs
    }
}