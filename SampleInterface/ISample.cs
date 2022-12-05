using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleInterface
{
    public interface ISample
    {
        string Name { get; }

        string Desc { get; }

        string Thumb { get; }

        void Setup(object[] parameters);

        void Start();

        void Stop();
    }
}
