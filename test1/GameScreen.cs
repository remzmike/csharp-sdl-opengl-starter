using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test1
{
    public class GameScreen
    {
        public virtual bool Init()
        {
            return true;
        }

        public virtual void Cleanup()
        {

        }

        public virtual void Update(float dt)
        {

        }
    }
}
