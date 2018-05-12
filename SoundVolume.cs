using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShackSoundboard
{
    struct SoundVolume
    {
        public float Linear { get; set; }

        [JsonIgnore]
        public float Decibel
        {
            get
            {
                return LinearToDecibel(Linear);
            }
            set
            {
                Linear = DecibelToLinear(value);
            }
        }

        public SoundVolume(float linear)
        {
            Linear = linear;
        }

        public static float LinearToDecibel(float value)
        {
            return (float)(20.0 * Math.Log10(value));
        }

        public static float DecibelToLinear(float value)
        {
            return (float)Math.Pow(10.0, value / 20.0);
        }

        public static SoundVolume operator*(SoundVolume left, SoundVolume right)
        {
            return new SoundVolume(left.Linear * right.Linear);
        }
    }
}
