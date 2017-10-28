using System;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Timers;

namespace DistrictRCI
{
    public class DistrictRCIPanel : UIPanel
    {
        public bool displayRequested;
        private int updateCount;

        public UISlicedSprite m_demandSprite;

        /**
         * FROM District Service Limit:
         * It's a bit tricky to get the building selection right:
         * 1) When selecting another building without deselecting the previous one, the OnVisibilityChanged
         * is not invoked, but just the OnPositionChanged.
         * 2) Even when the OnVisibilityChanged is called, when retrieving the current building within the
         * event handler, the previous building (or zero, if none was selected) will be retrieved instead. 
         * -----
         * In the next frame the building will have the correct value, so I'm using MonoBehavior's Update()
         * method here to wait the next tick and then retrieve the building with confidence.
         */
        public override void Update()
        {
            base.Update();
            if (displayRequested)
            {
                updateCount++;
                if (updateCount > 0)
                {
                    updateCount = 0;
                    displayRequested = false;

                    SetDemands(GetDistrict());
                }
            }
        }

        public void RefreshVisibility(bool show)
        {
            if (!show)
            {
                displayRequested = false;
            }
            else
            {
                displayRequested = true;
            }
        }

        public void OnVisibilityChanged(UIComponent component, bool visible)
        {
            RefreshVisibility(visible);
        }

        public void OnPositionChanged(UIComponent component, Vector2 position)
        {
            bool visible = DistrictRCIHook.m_UIPanel.isVisible;
            RefreshVisibility(visible);
        }


        private void SetDemands(District district)
        {
            foreach (UISlider slider in m_demandSprite.GetComponentsInChildren<UISlider>())
            {
                if (slider.name.Equals("ResidentialDemand"))
                {
                    slider.value = 0;
                    if (district.m_flags != District.Flags.None)
                        slider.value = CalculateResidentialDemand(district);
                }
                else if (slider.name.Equals("CommercialDemand"))
                {
                    slider.value = 0;
                    if (district.m_flags != District.Flags.None)
                        slider.value = CalculateCommercialDemand(district);
                }
                else if (slider.name.Equals("IndustryOfficeDemand"))
                {
                    slider.value = 0;
                    if (district.m_flags != District.Flags.None)
                        slider.value = CalculateWorkplaceDemand(district);
                }
            }
        }

        private int CalculateResidentialDemand(District districtData)
        {
            // from ZoneManager
            int a = (int)(districtData.m_commercialData.m_finalHomeOrWorkCount + districtData.m_industrialData.m_finalHomeOrWorkCount + districtData.m_officeData.m_finalHomeOrWorkCount + districtData.m_playerData.m_finalHomeOrWorkCount);
            int num = (int)(districtData.m_commercialData.m_finalEmptyCount + districtData.m_industrialData.m_finalEmptyCount + districtData.m_officeData.m_finalEmptyCount + districtData.m_playerData.m_finalEmptyCount);
            int finalHomeOrWorkCount = (int)districtData.m_residentialData.m_finalHomeOrWorkCount;
            int finalEmptyCount = (int)districtData.m_residentialData.m_finalEmptyCount;
            int num2 = (int)(districtData.m_educated0Data.m_finalUnemployed + districtData.m_educated1Data.m_finalUnemployed + districtData.m_educated2Data.m_finalUnemployed + districtData.m_educated3Data.m_finalUnemployed);
            int num3 = (int)(districtData.m_educated0Data.m_finalHomeless + districtData.m_educated1Data.m_finalHomeless + districtData.m_educated2Data.m_finalHomeless + districtData.m_educated3Data.m_finalHomeless);
            int num4 = Mathf.Clamp(100 - finalHomeOrWorkCount, 50, 100);
            num4 += Mathf.Clamp((num * 200 - num2 * 200) / Mathf.Max(a, 100), -50, 50);
            num4 += Mathf.Clamp((num3 * 200 - finalEmptyCount * 200) / Mathf.Max(finalHomeOrWorkCount, 100), -50, 50);
            //this.m_DemandWrapper.OnCalculateResidentialDemand(ref num4);
            Debug.Print(districtData.ToString() + " - R: " + num4);
            return Mathf.Clamp(num4, 0, 100);
        }

        private int CalculateCommercialDemand(District districtData)
        {
            // from ZoneManager
            int num = (int)(districtData.m_commercialData.m_finalHomeOrWorkCount - districtData.m_commercialData.m_finalEmptyCount);
            int num2 = (int)(districtData.m_residentialData.m_finalHomeOrWorkCount - districtData.m_residentialData.m_finalEmptyCount);
            int finalHomeOrWorkCount = (int)districtData.m_visitorData.m_finalHomeOrWorkCount;
            int finalEmptyCount = (int)districtData.m_visitorData.m_finalEmptyCount;
            int num3 = Mathf.Clamp(num2, 0, 50);
            num = num * 10 * 16 / 100;
            num2 = num2 * 20 / 100;
            num3 += Mathf.Clamp((num2 * 200 - num * 200) / Mathf.Max(num, 100), -50, 50);
            num3 += Mathf.Clamp((finalHomeOrWorkCount * 100 - finalEmptyCount * 300) / Mathf.Max(finalHomeOrWorkCount, 100), -50, 50);
            //this.m_DemandWrapper.OnCalculateCommercialDemand(ref num3);
            Debug.Print(districtData.ToString() + " - C: " + num3);
            return Mathf.Clamp(num3, 0, 100);
        }

        private int CalculateWorkplaceDemand(District districtData)
        {
            // from ZoneManager
            int value = (int)(districtData.m_residentialData.m_finalHomeOrWorkCount - districtData.m_residentialData.m_finalEmptyCount);
            int a = (int)(districtData.m_commercialData.m_finalHomeOrWorkCount + districtData.m_industrialData.m_finalHomeOrWorkCount + districtData.m_officeData.m_finalHomeOrWorkCount + districtData.m_playerData.m_finalHomeOrWorkCount);
            int num = (int)(districtData.m_commercialData.m_finalEmptyCount + districtData.m_industrialData.m_finalEmptyCount + districtData.m_officeData.m_finalEmptyCount + districtData.m_playerData.m_finalEmptyCount);
            int num2 = (int)(districtData.m_educated0Data.m_finalUnemployed + districtData.m_educated1Data.m_finalUnemployed + districtData.m_educated2Data.m_finalUnemployed + districtData.m_educated3Data.m_finalUnemployed);
            int num3 = Mathf.Clamp(value, 0, 50);
            num3 += Mathf.Clamp((num2 * 200 - num * 200) / Mathf.Max(a, 100), -50, 50);
            //this.m_DemandWrapper.OnCalculateWorkplaceDemand(ref num3);
            Debug.Print(districtData.ToString() + " - I: " + num3);
            return Mathf.Clamp(num3, 0, 100);
        }

        //private District GetDistrict(string name){
        private District GetDistrict()
        {
            Debug.Print("District active: " + ToolsModifierControl.policiesPanel.targetDistrict + " - " + Singleton<DistrictManager>.instance.GetDistrictName(ToolsModifierControl.policiesPanel.targetDistrict));
            if (ToolsModifierControl.policiesPanel.targetDistrict != 0)
                return Singleton<DistrictManager>.instance.m_districts.m_buffer[ToolsModifierControl.policiesPanel.targetDistrict];
            else
                return new District { m_flags = District.Flags.None };
        }
    }


	public class DistrictRCIHook : ILoadingExtension {

		public static UIPanel m_UIPanel;
        public static DistrictWorldInfoPanel m_DistrictWIP;
        public static DistrictRCIPanel rciPanel;
        public static GameObject rciGO;


		public void OnCreated(ILoading loading){
            /*
            Debug.Print("OnCreated()");
            DestroyOld ("DistrictRCIDemand");
            
			var timer = new Timer (10000); //delays GUI hook so other mods such as Building Themes can do its thing
			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				HookGUI();
				timer.Enabled = false;
				timer.Dispose();
			};

			timer.Enabled = true;            
            */
		}

		public void OnLevelLoaded(LoadMode mode){
			if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame)) {
				return;
			}
			Debug.Print ("OnLevelLoaded()");
			DestroyOld ("DistrictRCIDemand");

			var timer = new Timer (10000); //delays GUI hook so other mods such as Building Themes can do its thing
			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				HookGUI();
				timer.Enabled = false;
				timer.Dispose();
			};

			timer.Enabled = true;
		}

		private void DestroyOld(string name){
			try{
                if (rciGO != null)
				    GameObject.DestroyImmediate (rciGO);
			}catch{					
			}
		}

        public class DistrictRCIException : Exception
        {
            public DistrictRCIException(string msg) : base(msg) { }
        }

		private void HookGUI(){
			Debug.Print("Hooking GUI");

            m_DistrictWIP = UIView.library.Get<DistrictWorldInfoPanel>(typeof(DistrictWorldInfoPanel).Name);
            m_UIPanel = UIView.library.Get<UIPanel>(typeof(DistrictWorldInfoPanel).Name);

            rciGO = new GameObject("DistrictRCIDemand");
            rciPanel = rciGO.AddComponent<DistrictRCIPanel>();
            rciPanel.m_demandSprite = (UISlicedSprite)GameObject.Instantiate(UIView.Find<UISlicedSprite>("DemandBack"));

            if (m_DistrictWIP == null || m_UIPanel == null || rciPanel.m_demandSprite == null)
            {
                throw new DistrictRCIException("DistrictRCI couldn't hook GUI");
            }

            rciPanel.m_demandSprite.name = "DistrictRCIDemand_sprite";
            rciPanel.m_demandSprite.cachedName = "DistrictRCIDemand_sprite";

            m_UIPanel.AttachUIComponent(rciPanel.m_demandSprite.gameObject); //attach demand sprite directly to original UI panel
            rciPanel.m_demandSprite.relativePosition = new Vector3(m_UIPanel.width - rciPanel.m_demandSprite.width - 10f, m_UIPanel.height - rciPanel.m_demandSprite.height - 24f);
            rciPanel.m_demandSprite.Show();

            // custom panel only for triggering, always hide off-screen
            rciPanel.size = new Vector2(0, 0);
            rciPanel.transform.position = new Vector3(-10, 0, -10);
            rciPanel.Hide();
            m_UIPanel.eventVisibilityChanged += (component, value) => { rciPanel.OnVisibilityChanged(component, value); };
            m_UIPanel.eventPositionChanged += (component, value) => { rciPanel.OnPositionChanged(component, value); };
        }
        
		public void OnReleased(){
			Debug.Print ("Released");
			try{
                if (m_UIPanel != null && rciPanel != null)
			        m_UIPanel.RemoveUIComponent (rciPanel.m_demandSprite);
			}catch{			
			}
		}

		public void OnLevelUnloading(){}
	}
}

