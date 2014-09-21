using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ORSX //GOOD
{
    public interface IAnimatedModule
    {
        void EnableModule();
        void DisableModule();
        bool ModuleIsActive();
    }

    public class ORSX_AnimationGroup : PartModule
    {
        [KSPField]
        public string deployAnimationName = "Deploy";
        
        [KSPField]
        public string activeAnimationName = "Operate";

        [KSPField(isPersistant = true)]
        public bool isDeployed = false;

        [KSPField(isPersistant = false)] 
        public bool alwaysActive = false;

        [KSPField(isPersistant = true)]
        public bool _isActive;

        [KSPField(isPersistant = false)]
        public string moduleType = "Module";

        [KSPEvent(guiName = "Deploy", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void DeployModule()
        {
            SetDeployedState(1);
        }

        [KSPEvent(guiName = "Retract", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractModule()
        {
            SetRetractedState(-1);
        }

        [KSPAction("Deploy")]
        public void DeployModuleAction(KSPActionParam param)
        {
            if (!isDeployed)
            {
                DeployModule();
            }
        }

        [KSPAction("Retract")]
        public void RetractModuleAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractModule();
            }
        }


        [KSPAction("Toggle")]
        public void ToggleModuleAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractModule();
            }
            else
            {
                DeployModule();
            }
        }

        private List<IAnimatedModule> _Modules;

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }
        public Animation ActiveAnimation
        {
            get
            {
                if (activeAnimationName == "") return null;
                return part.FindModelAnimators(activeAnimationName)[0];
            }
        }

        public override void OnStart(StartState state)
        {
            FindModules();
            CheckAnimationState();
            DeployAnimation[deployAnimationName].layer = 3;
            if (activeAnimationName != "")
            {
                ActiveAnimation[activeAnimationName].layer = 4;
            }
            Setup();
        }

        private void Setup()
        {
            Events["DeployModule"].guiName = "Deploy " + moduleType;
            Events["RetractModule"].guiName = "Retract " + moduleType;
            Actions["DeployModuleAction"].guiName = "Deploy " + moduleType;
            Actions["RetractModuleAction"].guiName = "Retract " + moduleType;
            Actions["ToggleModuleAction"].guiName = "Toggle " + moduleType;
        }

        public override void OnLoad(ConfigNode node)
        {
            FindModules();
            CheckAnimationState();
        }

        public override void OnUpdate()
        {
            if (vessel != null)
            {
                if (!isDeployed)
                {
                    DisableModules();
                }
                else
                {
                    CheckForActivity();
                }
            }
            base.OnUpdate();
        }


        private void CheckAnimationState()
        {
            if (isDeployed)
            {
                SetDeployedState(1000);
            }
            else
            {
                SetRetractedState(-1000);                
            }
        }
        private void FindModules()
        {
            if (vessel != null)
            {
                if (part.Modules.OfType<IAnimatedModule>().Any())
                {
                    _Modules = part.Modules.OfType<IAnimatedModule>().ToList();
                }
            }
        }

        private void CheckForActivity()
        {
            if ((_Modules.Any(e => e.ModuleIsActive() || alwaysActive) && isDeployed))
            {
                if (!ActiveAnimation.isPlaying)
                {
                    ActiveAnimation[activeAnimationName].speed = 1;
                    ActiveAnimation.Play(activeAnimationName);
                }
            }
        }


        private void SetRetractedState(int speed)
        {
            isDeployed = false;
            Events["RetractModule"].active = false;
            Events["DeployModule"].active = true;
            PlayDeployAnimation(speed);
            DisableModules();
        }

        private void SetDeployedState(int speed)
        {
            isDeployed = true;
            Events["DeployModule"].active = false;
            Events["RetractModule"].active = true;
            PlayDeployAnimation(speed);
            EnableModules();
        }

        private void PlayDeployAnimation(int speed)
        {
            if (speed < 0)
            {
                if (activeAnimationName != "")
                {
                    ActiveAnimation.Stop(activeAnimationName);
                }

                DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            }
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);            
        }

        private void DisableModules()
        {
            if (vessel == null || _Modules == null) return; 
            foreach (var e in _Modules)
            {
                e.DisableModule();
            }
            _isActive = false;
        }

        private void EnableModules()
        {
            if (vessel == null || _Modules == null) return;
            foreach (var e in _Modules)
            {
                e.EnableModule();
            }
        }

    }
}
