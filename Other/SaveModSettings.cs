using System.Collections.Generic;

namespace PaleCourtCharms
{
    
    public class SaveModSettings
    {
        

        public bool[] newCharms = new bool[] { true, true, true, true };

        public bool[] gotCharms = new bool[] { false, false, false, false };

        public bool[] equippedCharms = new bool[] { false, false, false, false };

        public bool upgradedCharm_10 = false;
public List<int> notchCosts = new List<int>(new int[5]);

public List<int> EnabledCharms = new();


        public UnityEngine.Vector3 IndicatorPosition1;
        public UnityEngine.Vector3 IndicatorPosition2;
    }
}