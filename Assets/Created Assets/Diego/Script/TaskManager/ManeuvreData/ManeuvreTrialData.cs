using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData
{
    public enum PrecisionLevel { TIGHT= 0, COARSE};

    
    /**
    * This class describes a maneuvre task in the system, for construction purposes only.
    *  It simply contains the values of our independent variables: 
    *      - UserID: User performing the task (this will actually influence tast order in the sequence, etc. 
    *      - technique: Navigation technique used (NAVIFIELD or HOMOGENEOUS). [NOTE: Natural navigaiton will be identified as HOMOGENEOUS with M=1]
    *      - M_factor: Scaling factor applied TO THE GLOBAL NAVIGABLE FIELD). When using NAVIFIELD, we need to use the equivalent M_factor (see bellow)
    *      - M_factor_equivalent: When using Navifield, the value of Max_M for our interactive areas the yield average scaling equal to M_factor. 
    *          This depends on the size of the environment, size of the zones (ri and Ri) and the size of navigable space 
    *       - path: Path to be followed for the trial (from flag 1).    
    *       - randomFlagOffset: According to our experiment design, we randomise the flag order. We compute the random starting flag here
    *          We will compute each flag as: randomizedFlag=  (path[i]+randomFlagOffset)%6;  
    */
    public class ManeuvreTrialData
    {
        public int UserID;
        //A. Independent Variables
        public NavigationTechnique technique;
        public M_FACTOR M_factor;                 //Average K factor of all the Navigable space (it is the same for NAVIFIELD and HOMOGENEOUS)
        public class ParafrustumTrial
        {
            public int paraFrustumIndex;
            public PrecisionLevel head, tail;
            public static List<ParafrustumTrial> generateFullTrialSequence() {
                List<ParafrustumTrial> result = new List<ParafrustumTrial> ();
                Random r = new Random();
                for (int pos = 0; pos < 3; pos++) {
                    List<PrecisionLevel> _heads = (new PrecisionLevel[] { PrecisionLevel.COARSE, PrecisionLevel.COARSE, PrecisionLevel.TIGHT, PrecisionLevel.TIGHT }).ToList<PrecisionLevel>();
                    List<PrecisionLevel> _tails = (new PrecisionLevel[] { PrecisionLevel.COARSE, PrecisionLevel.TIGHT, PrecisionLevel.COARSE,  PrecisionLevel.TIGHT }).ToList<PrecisionLevel>();
                    for (int t = 0; t < 4; t++)
                    {
                        ParafrustumTrial curTrial = new ParafrustumTrial();
                        //Fill at random
                        curTrial.paraFrustumIndex = pos * 2 + r.Next(0, 2);
                        int _h_t_type = r.Next(0, _heads.Count);
                        curTrial.head = _heads[_h_t_type];
                        curTrial.tail = _tails[_h_t_type];
                        _heads.RemoveAt(_h_t_type);
                        _tails.RemoveAt(_h_t_type);
                        result.Add(curTrial);
                    }
                }

                return result;

            }
            public override string ToString() {
                return "" + paraFrustumIndex + "," + head + "," + tail ;
            }
        };
        public ParafrustumTrial[] trials=new ParafrustumTrial[3];//6

        public int curStep = 0;
        public class ParafrustumDependentVariables {
            public int numParafrfustumErrors = 0;//Number of times the user was in, but left the parafrustum by accident.    
            //We will report average errors, deviation,etc
            //We record absolute values and will divide later by the numSamples
            public float M_TCT= 0;
            public float M_TOTAL_ORIENT_ERROR = 0;
            public float M_TOTAL_POS_ERROR = 0;
            public int numSamples = 0;
            //Same, but only counting when eyes and orientation inside parafrustum.
            public float M_TOTAL_ORIENT_ERROR_IN = 0;
            public float M_TOTAL_POS_ERROR_IN = 0;
            public int numSamplesIn = 0;
        };
        //B. Dependent Variables (Results): 

        
        public ParafrustumDependentVariables globalVaraibles;
        public ParafrustumDependentVariables[] perTrialVariables;

        
        static Random r = new Random();
        public ManeuvreTrialData() {
            globalVaraibles = new ParafrustumDependentVariables();
            perTrialVariables = new ParafrustumDependentVariables[3];//6
            for(int i=0;i<3;i++)
                perTrialVariables[i]= new ParafrustumDependentVariables();

            /*head = new PrecisionLevel[6];
            tail = new PrecisionLevel[6];
            paraFrustumIndex = new int[6];
            //A.The first trial is always easy (we do not use it for the results...)
            head[0] = PrecisionLevel.COARSE;
            tail[0] = PrecisionLevel.COARSE;
            //B.The following four are the actual task, and these get randomized.
            //... this pool contains our four conditions (T_T, T_L, L_T, L_L)
            List<PrecisionLevel> _headPool = (new PrecisionLevel[] {PrecisionLevel.TIGHT,PrecisionLevel.TIGHT, PrecisionLevel.COARSE,PrecisionLevel.COARSE}).ToList<PrecisionLevel>();
            List<PrecisionLevel> _tailPool = (new PrecisionLevel[] {PrecisionLevel.TIGHT, PrecisionLevel.COARSE, PrecisionLevel.TIGHT, PrecisionLevel.COARSE }).ToList<PrecisionLevel>();
            
            int trialIndex;
            for (int i = 0; i < 4; i++)
            {
                //Pick an index to the sequence (at random)
                trialIndex=r.Next(0, _headPool.Count);
                //Assign that as the next parafrustum condition.
                head[i + 1] = _headPool[trialIndex];
                tail[i + 1] = _tailPool[trialIndex];
                //Remove-> no repeated conditions...
                _headPool.RemoveAt(trialIndex);
                _tailPool.RemoveAt(trialIndex);
            }*/

        }

        public static void createPairedManeuvreTrialData(ref ManeuvreTrialData first, ref ManeuvreTrialData second) {
            List<ParafrustumTrial> fullTrials = ParafrustumTrial.generateFullTrialSequence();
            Random r = new Random();
            //Generate and validate pattern:
            //A. generate a division at random
            int randomNumber;
            for (int i = 0; i < 3; i++) {//6
                randomNumber = r.Next(0, fullTrials.Count);
                first.trials[i] = fullTrials[randomNumber];
                fullTrials.RemoveAt(randomNumber);
            }
            for (int i = 0; i < 3; i++)//6
            {
                randomNumber = r.Next(0, fullTrials.Count);
                second.trials[i] = fullTrials[randomNumber];
                fullTrials.RemoveAt(randomNumber);
            }
            //B. Validate that no adjacent parafrusutms exists, and resuffle (recursively) if necessary
            for (int i = 1; i < 3; i++) {//6
                //If repeated pos in any of the sequences
                if (first.trials[i].paraFrustumIndex == first.trials[i - 1].paraFrustumIndex
                    || second.trials[i].paraFrustumIndex == second.trials[i - 1].paraFrustumIndex)
                {
                    createPairedManeuvreTrialData(ref first, ref second);
                    return;
                }
            }
            //C. Success! We stop recursion and return values
            return;
        }


        /***
         *When we write the results, we need to order the columns in a fixed order, related to their position (High/Med/Low) and its parafrustum condution (T_T,T_L, L_T, L_L):
         * <High_T_T> <High_T_L> <High_L_T> <High_L_L> <High_T_T> <Med_T_L> <Med_L_T> <Med_L_L> <Med_T_T> <Low_T_L> <Low_L_T> <Low_L_L> 
         * This function returns the index of a given trial in this column order
         */
        public int getParafrustumColumnOrderForTrial(int trialNumber) {
            int result = 0;

            ParafrustumTrial pt= this.trials[trialNumber];
            result = (pt.paraFrustumIndex < 2 ? 0 : (pt.paraFrustumIndex < 4 ? 4 : 8));
            result+= (pt.head==PrecisionLevel.TIGHT? 0:2);
            result += (pt.tail == PrecisionLevel.TIGHT ? 0 : 1);
            return result;
        }

        public void increaseParafrustumErrors(int curParafrustumTrial) {
            globalVaraibles.numParafrfustumErrors++;
            perTrialVariables[curParafrustumTrial].numParafrfustumErrors++;
        }

        //This is called once per frame. It updates both the global variables for the whole task and the trial related ones (T_T, T_L, L_T, LL)
        public void updateDependentVariables(int curTrial, float orientError, float posError) {
            globalVaraibles.M_TOTAL_ORIENT_ERROR += orientError;
            //globalVaraibles.M_TOTAL_ORIENT_ERROR += 1;
            globalVaraibles.M_TOTAL_POS_ERROR += posError;
            globalVaraibles.numSamples++;
            //Now let's go for the trial related ones. 
            perTrialVariables[curTrial].M_TOTAL_ORIENT_ERROR+= orientError;
            //perTrialVariables[curTrial].M_TOTAL_ORIENT_ERROR += 1;
            perTrialVariables[curTrial].M_TOTAL_POS_ERROR += posError;
            perTrialVariables[curTrial].numSamples++;
            
        }

        //This is called once per frame. It updates both the global variables for the whole task and the trial related ones (T_T, T_L, L_T, LL)
        public void updateDependentVariablesInsideParafrustum(int curTrial, float orientError, float posError)
        {
            globalVaraibles.M_TOTAL_ORIENT_ERROR_IN += orientError;
            globalVaraibles.M_TOTAL_POS_ERROR_IN += posError;
            globalVaraibles.numSamplesIn++;
            //Now let's go for the trial related ones. 
            perTrialVariables[curTrial].M_TOTAL_ORIENT_ERROR_IN += orientError;
            perTrialVariables[curTrial].M_TOTAL_POS_ERROR_IN += posError;
            perTrialVariables[curTrial].numSamplesIn++;
        }

        public void updateTrialTCT(int curTrial, float TCT) {
            perTrialVariables[curTrial].M_TCT = TCT;
        }

        public override string ToString()
        {
            string result = "User" + this.UserID + "," + technique + ", " + M_factor ;
            foreach (ParafrustumTrial p in this.trials)
            {
                result += "," + p.paraFrustumIndex +"," + p.head +","+ p.tail;
            }
            return result;
        }
    }
}
