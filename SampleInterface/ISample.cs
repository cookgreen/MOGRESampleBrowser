using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleInterface
{
    public interface ISample
    {
        event Action SampleStopped;

        string Name { get; }

        string Desc { get; }

        string Thumb { get; }

        void Setup(object[] parameters);

        void Start();

        void Pause();

        void Resume();

        void Stop();

        void Update(float timeSinceLastFrame);
    }
}
