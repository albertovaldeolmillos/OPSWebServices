using PDMDatabase.Models;
using PDMDatabase.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMDatabase.UnitTests.Fakes
{
    public class FakeConstraintsRepository : ConstraintsRepository
    {
        public FakeConstraintsRepository(IDbConnection connection) : base(connection)
        {
        }

        public override IEnumerable<Constraints> GetAll()
        {
            return new List<Constraints>()
            {
                new Constraints() { CON_ID=60101, CON_NUMBER = 1, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =1, CON_VALUE = 480  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 2, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =2, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 3, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =3, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 4, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =4, CON_VALUE = 500  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 5, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =5, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 6, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =6, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 7, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =7, CON_VALUE = 1  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 8, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =8, CON_VALUE = 1  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 9, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =9, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 10, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =10, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 11, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =11, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 12, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =12, CON_VALUE = 0  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 13, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =13, CON_VALUE = 1  },
                new Constraints() { CON_ID=60101, CON_NUMBER = 14, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =14, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 1, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =1, CON_VALUE = 480  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 2, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =2, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 3, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =3, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 4, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =4, CON_VALUE = 200  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 5, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =5, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 6, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =6, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 7, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =7, CON_VALUE = 1  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 8, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =8, CON_VALUE = 1  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 9, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =9, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 10, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =10, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 11, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =11, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 12, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =12, CON_VALUE = 0  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 13, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =13, CON_VALUE = 1  },
                new Constraints() { CON_ID=60201, CON_NUMBER = 14, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =14, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 1, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =1, CON_VALUE = 60  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 2, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =2, CON_VALUE = 60  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 3, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =3, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 4, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =4, CON_VALUE = 70  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 5, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =5, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 6, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =6, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 7, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =7, CON_VALUE = 1  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 8, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =8, CON_VALUE = 1  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 9, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =9, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 10, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =10, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 11, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =11, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 12, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =12, CON_VALUE = 0  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 13, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =13, CON_VALUE = 1  },
                new Constraints() { CON_ID=60301, CON_NUMBER = 14, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =14, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 1, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =1, CON_VALUE = 1440  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 2, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =2, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 3, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =3, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 4, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =4, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 5, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =5, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 6, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =6, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 7, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =7, CON_VALUE = 1  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 8, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =8, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 9, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =9, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 10, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =10, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 11, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =11, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 12, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =12, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 13, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =13, CON_VALUE = 0  },
                new Constraints() { CON_ID=60401, CON_NUMBER = 14, CON_DGRP_ID = 2, CON_GRP_ID= null, CON_DCON_ID =14, CON_VALUE = 0  }
            };
        }
    }
}
