using Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Created_Assets.Diego.Script.TaskManager
{
    /**
     * This class represents each trial in a sequence, describing both the Travelling task and the maneuvring task
         */
    public class TaskTrialData
    {
        public TravellingTrialData travellingTrialData;
        public ManeuvreTrialData maneuvringTrialData; 
        public TaskTrialData(TravellingTrialData t, ManeuvreTrialData m) { travellingTrialData = t; maneuvringTrialData = m; }
        public override string ToString()
        {
            return "Travel(" + travellingTrialData + ")\n";
        }
    }

    /**
     * This class constains a descriptor of all the trial description data, for a given UserID
     * It encodes our experiment desing: NaturalStart + FactorialDesign (2x3x2) + NaturalEnd  
     * METHODS: 
     *      - _fillUserSequences: Fills in a database encoding our experiment design
     *      - getTrialSequenceData: Returns a UserTrialSequenceData, encoding the order in which a user has to coomplete the tasks.  
    */
    public class UserTrialSequenceData
    {   //Atributes describing a User sequence of trials:
        List<TaskTrialData> trials;
        int curIndex;
        //Constructor is protected. UserTrialSequenceData can only be constructed through the static method getTrialSequenceData.
        protected UserTrialSequenceData() {
            curIndex = 0;
            trials = new List<TaskTrialData>();

        }
        public TaskTrialData getNextTrialData() {
            if (curIndex >= trials.Count)
                return null;
            return trials[curIndex++];//Return trial and increase pointer. 
        }
        public bool sequenceFinished() {
            return curIndex >= trials.Count;
        }
        public override string ToString()
        {
            string result = "";
            foreach (TaskTrialData t in trials)
                result += t.ToString();
            return result;
        }

        //Atributes to manage our database of valid sequences. 
        protected const int NUM_USERS = 12;
        protected static Random rnd = new Random();
        protected static Dictionary<int, UserTrialSequenceData> userSequences = null;

        //Call this method to get a description of the tasks to complete by a given userId.
        public static UserTrialSequenceData getTrialSequenceData(int userId)
        {
            if (userSequences == null)
                _fillUserSequences();
            return userSequences[userId % NUM_USERS];
        }

        //This protected method fills all sequences for all users, following our experiment design
        public static void _fillUserSequences()
        {
            userSequences = new Dictionary<int, UserTrialSequenceData>();

         int curUserId = 0;
        
         for (int t = 0; t < 2; t++)
             for (int m = 0; m < 3; m++)
                 for (int l = 2; l <= 4; l += 2, curUserId++)
                 {
                     NavigationTechnique aux = (NavigationTechnique)(t), aux2 = (NavigationTechnique)(1 - t);
                     NavigationTechnique[] techniques = { aux, aux2 };
                     M_FACTOR[] m_factors = { (M_FACTOR)(m), (M_FACTOR)((m + 1) % 3), (M_FACTOR)((m + 2) % 3) };
                     int[] lengths = { l, 6 - l };       //2-4 or 4-2
                     userSequences[curUserId] = _fillUserSequence(curUserId, techniques, m_factors, lengths);
                 }

        }

        /**This method fills in one sequence of trials, based on a especific arrangement of the factorial desing.
         * Our design is as follows (see above also): NaturalStart + FactorialDesign (2x3x2) + NaturalEnd  
            For instance _fillUserSequence(NavigationTechnique[] t={NAVIFIELD,HOMOGENEOUS}, M_FACTOR[] k={M_2,M_4,M_8}, int[] l={4,2}); 
         */
        protected static UserTrialSequenceData _fillUserSequence(int userId, NavigationTechnique[] techniques, M_FACTOR[] factors, int[] lengths)
        {            
            UserTrialSequenceData sequence = new UserTrialSequenceData();
            //2. Add factorial design 2x3x2 (according to parameters).
            //{
            //    foreach (NavigationTechnique t in techniques)
            //        for (int m = 0; m < 3; m++)
            //        {
            //            ManeuvreTrialData[] mDatas = new ManeuvreTrialData[2];
            //            for (int i = 0; i < 2; i++) mDatas[i] = new ManeuvreTrialData();
            //            ManeuvreTrialData.createPairedManeuvreTrialData(ref mDatas[0], ref mDatas[1]);
            //            int mDatasIndex = 0;

            //            foreach (int l in lengths)
            //            {
            //                //A. Build the new TravellingTrialData()
            //                TravellingTrialData tData = new TravellingTrialData();
            //                tData.UserID = userId;
            //                tData.technique = t; tData.M_factor = factors[m]; tData.length = l;
            //                //Get random path of length l; 
            //                tData.path = NavigationPathOrder.getRandomPathOfLength(tData.length);
            //                tData.randomFlagOffset = rnd.Next(0, 6);
            //                //B. Build nbew maneuvering data
            //                ManeuvreTrialData mData = mDatas[mDatasIndex++]; ;
            //                mData.UserID = userId;
            //                mData.technique = t; mData.M_factor = factors[m];
            //                //C. Add new task trial data to the sequence.
            //                sequence.trials.Add(new TaskTrialData(tData, mData));
            //            }

            //        }
            //}
            {
                foreach (int l in lengths)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        ManeuvreTrialData[] mDatas = new ManeuvreTrialData[2];
                        for (int i = 0; i < 2; i++) mDatas[i] = new ManeuvreTrialData();
                        ManeuvreTrialData.createPairedManeuvreTrialData(ref mDatas[0], ref mDatas[1]);
                        int mDatasIndex = 0;

                        foreach (NavigationTechnique t in techniques) 
                        {
                            //A. Build the new TravellingTrialData()
                            TravellingTrialData tData = new TravellingTrialData();
                            tData.UserID = userId;
                            tData.technique = t; tData.M_factor = factors[m]; tData.length = l;
                            //Get random path of length l; 
                            tData.path = NavigationPathOrder.getRandomPathOfLength(tData.length);
                            tData.randomFlagOffset = rnd.Next(0, 6);
                            //B. Build nbew maneuvering data
                            ManeuvreTrialData mData = mDatas[mDatasIndex++]; ;
                            mData.UserID = userId;
                            mData.technique = t; mData.M_factor = factors[m];
                            //C. Add new task trial data to the sequence.
                            sequence.trials.Add(new TaskTrialData(tData, mData));
                        }

                    }
                }
            }
            //1. Add the Natural baseline
            ManeuvreTrialData first = new ManeuvreTrialData(), second = new ManeuvreTrialData();
            ManeuvreTrialData.createPairedManeuvreTrialData(ref first, ref second);
            {//1.A. Build the new TravellingTrialData()
                TravellingTrialData tData = new TravellingTrialData();
                tData.UserID = userId;
                tData.technique = NavigationTechnique.HOMOGENEOUS;
                tData.M_factor = M_FACTOR.M_NONE;
                tData.length = lengths[0];
                //Get random path of length l; LENGTH IS CURRENTLY FIXED TO 2!
                tData.path = NavigationPathOrder.getRandomPathOfLength(tData.length);
                tData.randomFlagOffset = rnd.Next(0, 6);
                //1.B. Build nbew maneuvering data
                ManeuvreTrialData mData = first;
                mData.UserID = userId;
                mData.technique = NavigationTechnique.HOMOGENEOUS; ; mData.M_factor = M_FACTOR.M_NONE;

                //1.C. Add new task trial data to the sequence.
                sequence.trials.Add(new TaskTrialData(tData, mData));
            }
            //3. Add the NaturalEnd.
            {   //3.A. Build the new TravellingTrialData()
                TravellingTrialData tData = new TravellingTrialData();
                tData.UserID = userId;
                tData.technique = NavigationTechnique.HOMOGENEOUS;
                tData.M_factor = M_FACTOR.M_NONE;
                tData.length = lengths[1];
                //Get random path of length l; LENGTH IS CURRENTLY FIXED TO 2!
                tData.path = NavigationPathOrder.getRandomPathOfLength(tData.length);
                tData.randomFlagOffset = rnd.Next(0, 6);
                //1.B. Build nbew maneuvering data
                ManeuvreTrialData mData = second;
                mData.technique = NavigationTechnique.HOMOGENEOUS; ; mData.M_factor = M_FACTOR.M_NONE; 
                //1.C. Add new task trial data to the sequence.
                sequence.trials.Add(new TaskTrialData(tData,mData));
            }
            return sequence;
        }
    }
}
