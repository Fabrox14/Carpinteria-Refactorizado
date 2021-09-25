using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.accesoDatos
{
    abstract class AbstractDAOFactory
    {
        public abstract IPresupuestoDAO CrearPresupuestoDAO();
    }
}
