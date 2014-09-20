using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ORSX
{
    public class ORSX_AnimatedExtractor : PartModule
    {
        [KSPField]
        public string deployAnimationName = "Deploy";

        [KSPField]
        public string drillAnimationName = "Drill";

        [KSPField(isPersistant = true)]
        private bool isDeployed = false;

        [KSPField(isPersistant = true)]
        private bool _isDrilling;

        private StartState _state;

        [KSPEvent(guiName = "Deploy Drill", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void DeployDrill()
        {
            SetDeployedState(1);
        }

        [KSPEvent(guiName = "Retract Drill", guiActive = true, externalToEVAOnly = true, guiActiveEditor = true, active = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void RetractDrill()
        {
            SetRetractedState(-1);
        }

        [KSPAction("Deploy Drill")]
        public void DeployDrillAction(KSPActionParam param)
        {
            if (!isDeployed)
            {
                DeployDrill();
            }
        }

        [KSPAction("Retract Drill")]
        public void RetractDrillAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractDrill();
            }
        }


        [KSPAction("Toggle Drill")]
        public void ToggleDrillAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                RetractDrill();
            }
            else
            {
                DeployDrill();
            }
        }

        [KSPAction("Begin Extraction")]
        public void BeginExtractionAction(KSPActionParam param)
        {
            if (isDeployed && !_isDrilling)
            {
                ActivateExtractORSX_();
            }
        }

        [KSPAction("Stop Extraction")]
        public void StopExtractionAction(KSPActionParam param)
        {
            if (isDeployed && _isDrilling)
            {
                DisableExtractORSX_();
                EnableExtractORSX_();
            }
        }

        [KSPAction("Toggle Extraction")]
        public void ToggleExtractionAction(KSPActionParam param)
        {
            if (isDeployed)
            {
                if (_isDrilling)
                {
                    DisableExtractORSX_();
                    EnableExtractORSX_();
                }
                else
                {
                    ActivateExtractORSX_();
                }
            }
        }

        private List<ORSX_ModuleRailsExtraction> _extractORSX_;

        public Animation DeployAnimation
        {
            get
            {
                return part.FindModelAnimators(deployAnimationName)[0];
            }
        }
        public Animation DrillAnimation
        {
            get
            {
                if (drillAnimationName == "") return null;
                return part.FindModelAnimators(drillAnimationName)[0];
            }
        }

        public override void OnStart(StartState state)
        {
            _state = state;
            FindExtractORSX_();
            CheckAnimationState();
            DeployAnimation[deployAnimationName].layer = 3;
            if (drillAnimationName != "")
            {
                DrillAnimation[drillAnimationName].layer = 4;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            FindExtractORSX_();
            CheckAnimationState();
        }

        public override void OnUpdate()
        {
            if (vessel != null)
            {
                if (!isDeployed)
                {
                    DisableExtractORSX_();
                }
                else
                {
                    _isDrilling = _extractORSX_.Any(e => e.IsEnabled);
                    CheckForDrilling();
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
        private void FindExtractORSX_()
        {
            if (vessel != null)
            {
                if (part.Modules.Contains("ORSX_ModuleRailsExtraction"))
                {
                    _extractORSX_ = part.Modules.OfType<ORSX_ModuleRailsExtraction>().ToList();
                }
            }
        }

        private void CheckForDrilling()
        {
            if (_isDrilling)
            {
                if (!DrillAnimation.isPlaying)
                {
                    DrillAnimation[drillAnimationName].speed = 1;
                    DrillAnimation.Play(drillAnimationName);
                }
            }
        }


        private void SetRetractedState(int speed)
        {
            isDeployed = false;
            Events["RetractDrill"].active = false;
            Events["DeployDrill"].active = true;
            PlayDeployAnimation(speed);
            DisableExtractORSX_();
        }

        private void SetDeployedState(int speed)
        {
            isDeployed = true;
            Events["DeployDrill"].active = false;
            Events["RetractDrill"].active = true;
            PlayDeployAnimation(speed);
            EnableExtractORSX_();
        }

        private void PlayDeployAnimation(int speed)
        {
            if (speed < 0)
            {
                DeployAnimation[deployAnimationName].time = DeployAnimation[deployAnimationName].length;
            }
            DeployAnimation[deployAnimationName].speed = speed;
            DeployAnimation.Play(deployAnimationName);
        }

        private void DisableExtractORSX_()
        {
            if (vessel == null || _extractORSX_ == null) return;
            foreach (var e in _extractORSX_)
            {
                e.isEnabled = false;
                e.IsEnabled = false;
            }
            _isDrilling = false;
        }

        private void EnableExtractORSX_()
        {
            if (vessel == null || _extractORSX_ == null) return;
            foreach (var e in _extractORSX_)
            {
                e.isEnabled = true;
            }
        }

        private void ActivateExtractORSX_()
        {
            if (vessel == null || _extractORSX_ == null) return;
            foreach (var e in _extractORSX_)
            {
                e.IsEnabled = true;
            }
            _isDrilling = true;
        }
    }
}
