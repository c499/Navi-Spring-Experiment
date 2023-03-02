using Assets.Created_Assets.Diego.Script.TaskManager.ManeuvreData;
using Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.HelperTasks;
using Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation.TravelTaskImplementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Assets.Created_Assets.Diego.Script.TaskManager.UnityTaskImplementation
{
    class TaskTrialFactory
    {
        //This function controls the sequence along the task: starting + traveling + maneuvering + quetionnaires
        public static Task buildTaskTrial(TaskTrialData data) {            
            CompositeTask t = new CompositeTask(data);
            t.addTask(new ReturnToCentreTask2(data));
            t.addTask(new TravelTask2(data));
            t.addTask(new ManeuverTask(data));
            t.addTask(new QuestionnaireTask(data));

            return t;

        }

        class IntroductionTask_TunningT1 : IntroductionTask.IntroductionTask_Tunning
        {
            public override void allocateTask()
            {
                //Set natural navigation
                NavigationControl.instance().setMFactor(M_FACTOR.M_NONE);
                NavigationControl.instance().resetDrift();

                //Setup environment:
                EnvironmentManager rs = EnvironmentManager.instance();
                rs.setupTravelScene(M_FACTOR.M_NONE);
                rs.playEffect(SoundEffects.RELAX_FEEDBACK);
                //Show central area: 
                rs.showCentralArea(true);
                rs.highlightCentralArea(true);
                rs.showQuestionnaire(false);
                //Hide all flags            
                for (int f = 0; f < 6; f++) rs.showFlag(f, false);
            }

            public override void deallocateTask() {; }
        };

        class IntroductionTask_TunningT2 : IntroductionTask.IntroductionTask_Tunning
        {
            public override void allocateTask() {
                for (int f = 0; f < 6; f++) EnvironmentManager.instance().showFlag(f, true);
            }
            public override void deallocateTask() {; }
        };

        class IntroductionTask_TunningT3 : IntroductionTask.IntroductionTask_Tunning
        {
            public override void allocateTask()
            {
                EnvironmentManager.instance().showCentralArea(true);
                EnvironmentManager.instance().highlightCentralArea(true);
            }
            public override void deallocateTask() {
                EnvironmentManager.instance().showCentralArea(false); ;
            }
        };
        class IntroductionTask_TunningT5 : IntroductionTask.IntroductionTask_Tunning
        {
            public override void allocateTask()
            {
                EnvironmentManager.instance().showCentralArea(true);
            }
            public override void deallocateTask()
            {
                ;
            }
        };
        class IntroductionTask_TunningT6 : IntroductionTask.IntroductionTask_Tunning
        {
            public override void allocateTask() {
                EnvironmentManager.instance().playEffect(SoundEffects.TICK_TACK_FEEDBACK);
            }
            public override void deallocateTask() {
                EnvironmentManager.instance().stopEffect(SoundEffects.TICK_TACK_FEEDBACK);
            }
        };
        
        public static Task buildIntroductoryTaask(TaskTrialData training) {
            TaskTrialData emptyTaskDescriptor = new TaskTrialData(new TravellingTrialData(), new ManeuvreTrialData());
            CompositeTask t = new CompositeTask(emptyTaskDescriptor);
            //Let's build the introductory tasks:
            //1. Some presentation and instructions
            string[] texts1;
            if(EnvironmentManager.instance().english)
                texts1= new string[] {
                "Hi! Welcome and thanks for taking part in our experiment.\n\nPress TRIGGER to continue...",
                "I am the \"Central Area\", and I will give you directions to help you during the experiment.\nPress TRIGGER to continue...",
                "When you see me in the centre of the room, please come to me for directions.\nPress TRIGGER to continue...",
                "I can be a bit naughty.If you cannot read me, I might be right behind you!\nPress TRIGGER to continue...",
                "We first need to take some measurements from you.\n It takes no time...\nPress TRIGGER to continue..." };
            else
                texts1 = new string[] {
                "Hola! Bienvenido y gracias por participar en nuestro experimento.\n\nPulsa TRIGGER para continuar...",
                "Soy el \"Area Central\", y te dare instrucciones para ayudarte durante el experimento.\nPulsa TRIGGER para continuar...",
                "Cuando me veas en el centro de la sala, acercate para recibir instrucciones.\nPulsa TRIGGER para continuar...",
                "A veces soy un poco traviesa. Si no me ves, puede que este justo detras tuyo!\nPulsa TRIGGER para continuar...",
                "Pero primero, tenemos que tomarte algunas medidas. \n Es un momentito...\nPulsa TRIGGER para continuar..." };
            t.addTask(new IntroductionTask(texts1, emptyTaskDescriptor, new IntroductionTask_TunningT1()));
            
            //2. User Calibration
            t.addTask(new UserCalibrationTask(emptyTaskDescriptor));

            //3. Some more instructions, to explain travel
            string[] texts2;
            if(EnvironmentManager.instance().english)
                texts2 = new string[] {
                "Great! Now let's get you ready for some action.\nDo you see those flags around you?\nPress TRIGGER to continue...",
                "OK! During the experiment you will need to travel to some of those flags.\nPress TRIGGER to continue...",
                "I will give you directions on how to do this. Let's give it a try...\nPress TRIGGER to continue..." };
            else
                texts2 = new string[] {
                "Genial! Ahora vamos a prepararte para la accion.\nVes esas banderas a tu alrededor?\nPulsa TRIGGER para continuar...",
                "Bien! Durante el experimento tienens que ir a esas banderas.\nPulsa TRIGGER para continuar...",
                "Te dare instrucciones de como hacerlo. Vamos a hacer una prueba...\nPulsa TRIGGER para continuar..." };


            t.addTask(new IntroductionTask(texts2, emptyTaskDescriptor, new IntroductionTask_TunningT2()));

            //4. Two navigation tasks
            training.maneuvringTrialData.UserID = 100;
            training.travellingTrialData.UserID = 100;

            t.addTask(new TravelTask2(training));
            /*string[] texts3 = new string[] {
                "Great! Let's try again.\nPress TRIGGER to continue..." };
            t.addTask(new IntroductionTask(texts3, emptyTaskDescriptor, new IntroductionTask_TunningT3()));
            t.addTask(new TravelTask2(trainingTravel2));*/

            #region recover later
            //5. Explain maneuvre:
            string[] texts4;
            if(EnvironmentManager.instance().english)
                texts4 = new string[] {
                "Brilliant! You got a hang of it. However, there is something else I need you to do for me.\nPress TRIGGER to continue...",
                "Now, when you finish your trip, I will ask you to try and align your head, to look at some targets.\nPress TRIGGER to continue...",
                "You will see two little spheres(red and blue), where you need to place your eyes.\nPress TRIGGER to continue...",
                "And you will also see a circle. You need to look at it's centre (it will turn green), and stay there for a bit.\nPress TRIGGER to continue...",
                "Let's give it a try... go to flag 3 and hit 6 targets.\nREMEMBER: Spheres for your eyes, and look at the circle.\nPress TRIGGER to continue..."};
            else
                texts4 = new string[] {
                "Estupendo! Le has cogido el tranquillo. Sin embargo, hay algo mas que necesito que hagas.\nPulsa TRIGGER para continuar...",
                "Pues bien, cuando termines tu ruta, quiero que intentes alinear tu cabeza, para mirar a algunos objetos desde un punto concreto.\nPulsa TRIGGER para continuar...",
                "Veras dos pequeñas bolitas (roja y azul), donde debes situar tus ojos.\nPulsa TRIGGER para seguir...",
                "Y tambien veras un circulo. Tienes que mirar a su centro (se pondra verde), y mantener tu posicion un poco. \nPulsa TRIGGER para continuar...",
                "Vamos a probar... ve a la bandera 3 y entrena con 6 puntos.\nRECUERDA: Bolitas para tus ojos y mira al circulo.\nPulsa TRIGGER para continuar..."};


            t.addTask(new IntroductionTask(texts4, emptyTaskDescriptor, new IntroductionTask_TunningT3()));
            t.addTask(new ManeuverTask(training));

            string[] texts5;
            if(EnvironmentManager.instance().english)
                texts5 = new string[] {
                "Brilliant! you seem to be ready.\n\nPress TRIGGER to continue..." };
            else 
                texts5 = new string[] {
                "Genial! Parece que estas preparado/a.\n\nPulsa TRIGGER para continuar..." };
            t.addTask(new IntroductionTask(texts5, emptyTaskDescriptor, new IntroductionTask_TunningT5()));

            string[] texts6;
            if(EnvironmentManager.instance().english)
                texts6 = new string[] {
                "One more thing. At times you will hear we turn on our timer, to measure your performance.\nPress TRIGGER to continue...",
                "Try to do your best when the timer is on!\n\nPress TRIGGER to continue...",
                "OK! Little less conversation and a little action. \n\nPress TRIGGER to START EXPERIMENT..."};
            else
                texts6 = new string[] {
                "Una cosa mas. A veces oiras que encendemos nuestro cronometro, para medir tu rendimiento.\nPulsa TRIGGER para continuar...",
                "Esfuerzate a tope cuando el cronometro este encendido!\n\nPulsa TRIGGER para continuar...",
                "Bueno, basta ya de chachara, no?. \n\nPulsa TRIGGER para COMENZAR EL EXPERIMENTO..."};

            
            t.addTask(new IntroductionTask(texts6, emptyTaskDescriptor, new IntroductionTask_TunningT6()));
            #endregion
            return t;
        }
    }
}
