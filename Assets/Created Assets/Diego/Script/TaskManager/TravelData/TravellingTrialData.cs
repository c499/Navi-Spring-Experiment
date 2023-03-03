using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Assets.Created_Assets.Diego.Script.TaskManager
{
    public enum NavigationTechnique { NAVIFIELD=0, HOMOGENEOUS, SPRINGGRID};
    //We will have one unique ID for eahc potential path in the experiment (2_1; 1_2; 2-1-2-1; 1-2-1-2 and symetrics. 
    public enum NavigationPathsIDs { NP2_1_A=0, NP2_1_B,               //2_1 paths (two possible paths starting on flag 0)
                              NP2_1_A_Sym, NP2_1_B_Sym,         //Symetric condition (do 5-flag for the flag order...)
                              NP1_2_A, NP1_2_B,                 //1_2 paths (two possible paths starting on flag 0)
                              NP1_2_A_Sym, NP1_2_B_Sym,         //Symetric condition (do 5-flag for the flag order...)
                              //4 step trajectories:
                              //2-1-2-1 paths (8)
                              NP2_1_2_1_A, NP2_1_2_1_B, NP2_1_2_1_C, NP2_1_2_1_D, NP2_1_2_1_E, NP2_1_2_1_F, NP2_1_2_1_G, NP2_1_2_1_H,
                              NP2_1_2_1_A_Sym, NP2_1_2_1_B_Sym, NP2_1_2_1_C_Sym, NP2_1_2_1_D_Sym, NP2_1_2_1_E_Sym, NP2_1_2_1_F_Sym, NP2_1_2_1_G_Sym, NP2_1_2_1_H_Sym,
                              //1-2-1-2 paths (8)
                              NP1_2_1_2_A, NP1_2_1_2_B, NP1_2_1_2_C, NP1_2_1_2_D, NP1_2_1_2_E, NP1_2_1_2_F, NP1_2_1_2_G, NP1_2_1_2_H,
                              NP1_2_1_2_A_Sym, NP1_2_1_2_B_Sym, NP1_2_1_2_C_Sym, NP1_2_1_2_D_Sym, NP1_2_1_2_E_Sym, NP1_2_1_2_F_Sym, NP1_2_1_2_G_Sym, NP1_2_1_2_H_Sym,

    }
    public enum M_FACTOR {M_2=0,  M_4=1, M_8=2, M_NONE=4}                //NOTE: M_1 is only used for natural navigation (baseline condition)

   
    /**
    * Class containing a unique Path ID and the associated flag order. 
    *     It acts as a Singleton, providing a static method to retrieve the order, given a NavigationPathID.
         */
    public class NavigationPathOrder : IComparable<NavigationPathOrder> {
        NavigationPathsIDs id;
        List<int> flagOrder;
        public NavigationPathOrder(NavigationPathsIDs id, List<int> flagOrder)
        {
            this.id = id;
            this.flagOrder = new List<int>(flagOrder);//Copy the list (works as it is a list of basic types. Otherwise, you need to Clone) http://stackoverflow.com/questions/222598/how-do-i-clone-a-generic-list-in-c
        }
        public int getFlagFromOrder(int order) {
            if (order < flagOrder.Count)
                return flagOrder[order];
            return -1;
        }

        public bool hasFlag(int f) {
            return flagOrder.Contains(f);
        }
        //This method is required by the IComparable
        //interface. 
        public int CompareTo(NavigationPathOrder other)
        {
            if (other == null)
            {
                return 1;
            }
            //Return the difference in id.
            return (id-other.id);
        }

        public override string ToString() {
            string result = "Path_" + this.id;
            //We always report five flags, even if the path is shorter (for simplicity processing the output file)
            int flagCount = 0;
            foreach (int f in flagOrder)
            {
                flagCount++;
                result += "," + f;
            }
            for (; flagCount < 5; flagCount++) {
                result += ", -1";//The remaining flags are set to -1
            }
            return result;
        }

        //STATIC METHODS PROVIDING CENTRALIZED ACCESS TO PATH DESCRIPTIONS:
        static Dictionary<NavigationPathsIDs, NavigationPathOrder> navigationPaths;
        public static NavigationPathOrder getPathOrder(NavigationPathsIDs id) {
            if (navigationPaths == null) {
                _fillNavigationPaths();
            }
            return navigationPaths[id];
        }

        public static NavigationPathOrder getRandomPathOfLength(int l) {
            int low= (int)NavigationPathsIDs.NP2_1_A, high= (int)NavigationPathsIDs.NP1_2_1_2_H_Sym;
            switch (l) {
                case 2:
                    low = (int)(NavigationPathsIDs.NP2_1_A);
                    high = (int)(NavigationPathsIDs.NP1_2_B_Sym);
                    break;
                case 4:
                    low = (int)(NavigationPathsIDs.NP2_1_2_1_A);
                    high = (int)(NavigationPathsIDs.NP1_2_1_2_H_Sym);
                    break;
            }
            return NavigationPathOrder.getPathOrder((NavigationPathsIDs)(rnd.Next(low, high+ 1)));
        }
        static Random rnd = new Random();
        public static void _fillNavigationPaths() {
            navigationPaths = new Dictionary<NavigationPathsIDs, NavigationPathOrder>();
            //2 step trajectories
            navigationPaths.Add(NavigationPathsIDs.NP2_1_A,     new NavigationPathOrder(NavigationPathsIDs.NP2_1_A,     (new int[] { 0, 2, 1 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_B,     new NavigationPathOrder(NavigationPathsIDs.NP2_1_B,     (new int[] { 0, 2, 3 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_A_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_A_Sym, (new int[] { 0, 4, 5 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_B_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_B_Sym, (new int[] { 0, 4, 3 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_A,     new NavigationPathOrder(NavigationPathsIDs.NP1_2_A,     (new int[] { 0, 1, 3 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_B,     new NavigationPathOrder(NavigationPathsIDs.NP1_2_B,     (new int[] { 0, 1, 5 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_A_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_A_Sym, (new int[] { 0, 5, 3 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_B_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_B_Sym, (new int[] { 0, 5, 1 }).ToList<int>()));
            //4 step trajectories:
            //2_1_2_1
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_A, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_A, (new int[] { 0, 2, 1 , 5, 4}).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_B, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_B, (new int[] { 0, 2, 1, 5, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_C, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_C, (new int[] { 0, 2, 1, 3, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_D, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_D, (new int[] { 0, 2, 1, 3, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_E, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_E, (new int[] { 0, 2, 3, 5, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_F, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_F, (new int[] { 0, 2, 3, 5, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_G, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_G, (new int[] { 0, 2, 3, 1, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_H, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_H, (new int[] { 0, 2, 3, 1, 2 }).ToList<int>()));

            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_A_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_A_Sym, (new int[] { 0, 4, 5, 1, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_B_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_B_Sym, (new int[] { 0, 4, 5, 1, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_C_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_C_Sym, (new int[] { 0, 4, 5, 3, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_D_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_D_Sym, (new int[] { 0, 4, 5, 3, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_E_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_E_Sym, (new int[] { 0, 4, 3, 1, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_F_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_F_Sym, (new int[] { 0, 4, 3, 1, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_G_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_G_Sym, (new int[] { 0, 4, 3, 5, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP2_1_2_1_H_Sym, new NavigationPathOrder(NavigationPathsIDs.NP2_1_2_1_H_Sym, (new int[] { 0, 4, 3, 5, 4 }).ToList<int>()));

            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_A, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_A, (new int[] { 0, 1, 3, 2, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_B, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_B, (new int[] { 0, 1, 3, 2, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_C, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_C, (new int[] { 0, 1, 3, 4, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_D, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_D, (new int[] { 0, 1, 3, 4, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_E, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_E, (new int[] { 0, 1, 5, 4, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_F, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_F, (new int[] { 0, 1, 5, 4, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_G, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_G, (new int[] { 0, 1, 5, 0, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_H, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_H, (new int[] { 0, 1, 5, 0, 4 }).ToList<int>()));

            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_A_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_A_Sym, (new int[] { 0, 5, 3, 4, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_B_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_B_Sym, (new int[] { 0, 5, 3, 4, 2 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_C_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_C_Sym, (new int[] { 0, 5, 3, 2, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_D_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_D_Sym, (new int[] { 0, 5, 3, 2, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_E_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_E_Sym, (new int[] { 0, 5, 1, 2, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_F_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_F_Sym, (new int[] { 0, 5, 1, 2, 0 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_G_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_G_Sym, (new int[] { 0, 5, 1, 0, 4 }).ToList<int>()));
            navigationPaths.Add(NavigationPathsIDs.NP1_2_1_2_H_Sym, new NavigationPathOrder(NavigationPathsIDs.NP1_2_1_2_H_Sym, (new int[] { 0, 5, 1, 0, 2 }).ToList<int>()));
        }

    }

    /**
     * This class describes a travelling task in the system, for construction purposes only.
     *  It simply contains the values of our independent variables: 
     *      - UserID: User performing the task (this will actually influence tast order in the sequence, etc. 
     *      - technique: Navigation technique used (NAVIFIELD or HOMOGENEOUS). [NOTE: Natural navigaiton will be identified as HOMOGENEOUS with M=1]
     *      - M_factor: Scaling factor applied TO THE GLOBAL NAVIGABLE FIELD). When using NAVIFIELD, we need to use the equivalent M_factor (see bellow)
     *      - M_factor_equivalent: When using Navifield, the value of Max_M for our interactive areas the yield average scaling equal to M_factor. 
     *          This depends on the size of the environment, size of the zones (ri and Ri) and the size of navigable space 
     *          NEEDS SOLVING: 
     *              ->Fix environment size for all three conditions M=2,4,8; 
     *              -> Compute navigable field
     *              -> Compute equivalent M factor. 
     *              -> Check the solution actually allows us to cover the navigable space. 
     *       - path: Path to be followed for the trial (from flag 1).    
     *       - randomFlagOffset: According to our experiment design, we randomise the flag order. We compute the random starting flag here
     *          We will compute each flag as: randomizedFlag=  (path[i]+randomFlagOffset)%6;  
     */
    public class TravellingTrialData
    {
        public int UserID;
        //A. Independent Variables
        public NavigationTechnique technique;
        public M_FACTOR M_factor;                 //Average K factor of all the Navigable space (it is the same for NAVIFIELD and HOMOGENEOUS)
        public float M_factor_equivalent;      //Maximum K factor of the interactive areas.  
        public int length;
        public NavigationPathOrder path;
        public int curStep =0;
        public int randomFlagOffset;
        //B. Dependent Variables (Results): 
        public float T_TCT=0;
        //We will report average errors, deviation,etc
        //We record absolute values and will divide later by the numSamples
        public int numSamples=0;             
        public float totalDeviation=0;
        public float realDistanceTravelled=0, virtualDistanceTravelled=0;

        public override string ToString()
        {
            string result = "U_" + this.UserID + ", T_" + technique + ", M_"+M_factor+", M_Eq_"+M_factor_equivalent+",L_"+length+",("+path+"), Flag_Offset_"+randomFlagOffset;
            return result;
        }
    }

}


