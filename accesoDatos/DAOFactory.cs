using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpinteria_Refactorizado.accesoDatos
{
    class DAOFactory : AbstractDAOFactory
    {
        public override IPresupuestoDAO CrearPresupuestoDAO()
        {
            return new PresupuestoDAO();
        }
    }
}
