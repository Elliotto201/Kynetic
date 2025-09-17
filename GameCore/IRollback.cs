using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IRollback
    {
        public object SaveSnapshot();
        public void LoadSnapshot(object snapshot);
    }
}
