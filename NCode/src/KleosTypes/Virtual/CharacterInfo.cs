using KleosTypes.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode.KleosTypes.Virtual
{
    [Serializable]
    public class CharacterInfo
    {
        public enum Gender
        {
            Male,
            Female,
            Undefined
        }
        public Gender gender;
        public List<M3DBlendshapeValue> BlendShapes = new List<M3DBlendshapeValue>();
    }
}
