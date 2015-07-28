using System;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Timers;

namespace DistrictRCI
{
	public class DistrictRCIHook : ILoadingExtension{

		static UIPanel m_panel;
		static UISlicedSprite m_demandSprite;

		public void OnCreated(ILoading loading){
			DestroyOld ("DistrictRCIDemand");
			var timer = new Timer (10000); //delays GUI hook so other mods such as Building Themes can do its thing
			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				HookGUI();
				timer.Enabled = false;
				timer.Dispose();
			};
			timer.Enabled = true;
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
			while (true) {
				try{
					GameObject.DestroyImmediate (UIView.Find(name).gameObject);
					Debug.Print ("Destroyed");
				}catch{					
					break;
				}
			}
		}

		private void HookGUI(){
			//m_distictName = UIView.Find<UITextField>("DistrictName");
			Debug.Print("Hooking");
			m_panel = (UIPanel)UIView.Find<UITextField>("DistrictName").parent.parent;
			//m_panel = UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
			Debug.Print (m_panel.cachedName);
			m_demandSprite = (UISlicedSprite)GameObject.Instantiate(UIView.Find<UISlicedSprite> ("DemandBack"));

			m_demandSprite.name = "DistrictRCIDemand";
			m_demandSprite.cachedName = "DistrictRCIDemand";
			m_demandSprite.Show();

			m_panel.AttachUIComponent(m_demandSprite.gameObject);
			m_demandSprite.relativePosition = new Vector3(m_panel.width - m_demandSprite.width - 10f,m_panel.height - m_demandSprite.height - 24f);
			m_panel.eventVisibilityChanged += (component, value) => {Update();};
			//m_panel.eventAnchorChanged += (component, value) => {Update();};
			//m_panel.eventPositionChanged += (component, value) => {Update();};
			//m_panel.eventMouseMove += (component, value) => {Update();};
			UIView.Find<UITextField> ("DistrictName").eventTextChanged += (component, value) => {Update();};
		}

		private void Update(){
			Debug.Print ("Update");
			Debug.Print (m_demandSprite.absolutePosition);

			Debug.Print (m_panel.absolutePosition);
			//m_demandSprite.absolutePosition = Vector3.zero;
			m_demandSprite.relativePosition = new Vector3(m_panel.width - m_demandSprite.width - 10f,m_panel.height - m_demandSprite.height - 24f);

			SetDemands (GetDistrict());
		}

		private void SetDemands(District district){
			foreach (UISlider slider in m_demandSprite.GetComponentsInChildren<UISlider> ()) {				
				if (slider.name.Equals ("ResidentialDemand")) {
					slider.value = CalculateResidentialDemand(district);
				}else if (slider.name.Equals ("CommercialDemand")) {
					slider.value = CalculateCommercialDemand(district);
				}else if (slider.name.Equals ("IndustryOfficeDemand")) {
					slider.value = CalculateWorkplaceDemand(district);
				}
			}
		}

		private int CalculateResidentialDemand(District districtData)
		{
			int a1 = (int) districtData.m_commercialData.m_finalHomeOrWorkCount + (int) districtData.m_industrialData.m_finalHomeOrWorkCount + (int) districtData.m_officeData.m_finalHomeOrWorkCount + (int) districtData.m_playerData.m_finalHomeOrWorkCount;
			int num1 = (int) districtData.m_commercialData.m_finalEmptyCount + (int) districtData.m_industrialData.m_finalEmptyCount + (int) districtData.m_officeData.m_finalEmptyCount + (int) districtData.m_playerData.m_finalEmptyCount;
			int a2 = (int) districtData.m_residentialData.m_finalHomeOrWorkCount;
			int num2 = (int) districtData.m_residentialData.m_finalEmptyCount;
			int num3 = (int) districtData.m_educated0Data.m_finalUnemployed + (int) districtData.m_educated1Data.m_finalUnemployed + (int) districtData.m_educated2Data.m_finalUnemployed + (int) districtData.m_educated3Data.m_finalUnemployed;
			int num4 = (int) districtData.m_educated0Data.m_finalHomeless + (int) districtData.m_educated1Data.m_finalHomeless + (int) districtData.m_educated2Data.m_finalHomeless + (int) districtData.m_educated3Data.m_finalHomeless;
			int demand = Mathf.Clamp(100 - a2, 50, 100) + Mathf.Clamp((num1 * 200 - num3 * 200) / Mathf.Max(a1, 100), -50, 50) + Mathf.Clamp((num4 * 200 - num2 * 200) / Mathf.Max(a2, 100), -50, 50);
			return Mathf.Clamp(demand, 0, 100);
		}

		private int CalculateCommercialDemand(District districtData)
		{
			int num1 = (int) districtData.m_commercialData.m_finalHomeOrWorkCount - (int) districtData.m_commercialData.m_finalEmptyCount;
			int num2 = (int) districtData.m_residentialData.m_finalHomeOrWorkCount - (int) districtData.m_residentialData.m_finalEmptyCount;
			int a1 = (int) districtData.m_visitorData.m_finalHomeOrWorkCount;
			int num3 = (int) districtData.m_visitorData.m_finalEmptyCount;
			int num4 = Mathf.Clamp(num2, 0, 50);
			int a2 = num1 * 10 * 16 / 100;
			int num5 = num2 * 20 / 100;
			int demand = num4 + Mathf.Clamp((num5 * 200 - a2 * 200) / Mathf.Max(a2, 100), -50, 50) + Mathf.Clamp((a1 * 100 - num3 * 300) / Mathf.Max(a1, 100), -50, 50);
			return Mathf.Clamp(demand, 0, 100);
		}

		private int CalculateWorkplaceDemand(District districtData)
		{
			int num1 = (int) districtData.m_residentialData.m_finalHomeOrWorkCount - (int) districtData.m_residentialData.m_finalEmptyCount;
			int a = (int) districtData.m_commercialData.m_finalHomeOrWorkCount + (int) districtData.m_industrialData.m_finalHomeOrWorkCount + (int) districtData.m_officeData.m_finalHomeOrWorkCount + (int) districtData.m_playerData.m_finalHomeOrWorkCount;
			int num2 = (int) districtData.m_commercialData.m_finalEmptyCount + (int) districtData.m_industrialData.m_finalEmptyCount + (int) districtData.m_officeData.m_finalEmptyCount + (int) districtData.m_playerData.m_finalEmptyCount;
			int num3 = (int) districtData.m_educated0Data.m_finalUnemployed + (int) districtData.m_educated1Data.m_finalUnemployed + (int) districtData.m_educated2Data.m_finalUnemployed + (int) districtData.m_educated3Data.m_finalUnemployed;
			int demand = Mathf.Clamp(num1, 0, 50) + Mathf.Clamp((num3 * 200 - num2 * 200) / Mathf.Max(a, 100), -50, 50);
			return Mathf.Clamp(demand, 0, 100);
		}

		//private District GetDistrict(string name){
		private District GetDistrict(){
			var districtID = ToolsModifierControl.policiesPanel.targetDistrict;
			DistrictManager manager = Singleton<DistrictManager>.instance;
			try{
				return manager.m_districts.m_buffer[districtID];
			}catch(Exception){
				Debug.Print ("Something went wrong trying to return district of ID: " + districtID.ToString());
			}
			//int i = 0;
			//while(i < 128){
			//	string districtName = manager.GetDistrictName(i);
			//	if (districtName != null) {
			//		if (districtName != null && districtName.Equals (name)) {
			//			return manager.m_districts.m_buffer [i];
			//		}
			//	}
			//	i++;
			//}
			return manager.m_districts.m_buffer[0]; 
		}

		public void OnReleased(){
			Debug.Print ("Released");
			try{
			m_panel.RemoveUIComponent (m_demandSprite);
			}catch{
			
			}
		}
		public void OnLevelUnloading(){}
	}
}

