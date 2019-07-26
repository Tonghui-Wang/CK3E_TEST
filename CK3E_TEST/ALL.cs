using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK3E_TEST
{
    internal class ALL
    {
        private ContactCk3e ck3e;

        public ContactCk3e CK3E
        {
            get { return ck3e; }
        }

        private static ALL instance;
        private static object _lock = new object();

        private ALL()
        {
            ck3e = new ContactCk3e();
        }

        public static ALL GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new ALL();
                    }
                }
            }
            return instance;
        }
    }
}