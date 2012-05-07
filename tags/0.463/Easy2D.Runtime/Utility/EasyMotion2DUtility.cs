using UnityEngine;
using System;



namespace EasyMotion2D
{

    public partial class EasyMotion2DUtility
    {
        static public int GetHashCode(string str)
        {
            //return BKDRHash(str);
            return str.GetHashCode();
        }

        // BKDR Hash 
        static int BKDRHash(string str)
        {
	        int seed = 131; // 31 131 1313 13131 131313 etc..
	        int hash = 0;
 
	        foreach( char c in str)
	        {
		        hash = hash * seed + c;
	        }
 
	        return (hash & 0x7FFFFFFF);
        }

    }


}