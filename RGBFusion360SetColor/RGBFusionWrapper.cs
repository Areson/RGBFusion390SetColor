
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using RGBFusion390SetColor.Animation;
using SelLEDControl;

namespace RGBFusion390SetColor
{
    public class RgbFusion
    {
        private Comm_LED_Fun _ledFun;
        private bool _areaChangeApplySuccess;        
        private List<CommUI.Area_class> _allAreaInfo;
        private List<CommUI.Area_class> _allExtAreaInfo;
        private Dictionary<int, CommUI.Area_class> _extAreaInfoLookup;
        //private List<LedCommand> _commands;
        private Thread _newChangeThread;
        private readonly sbyte _changeOperationDelay = 60;
        private bool _initialized;
        private AsyncQueue<LedCommand[]> _commands;
        private CancellationToken cancellationToken;
        private ManualResetEvent _initEvent;

        private AnimationHandler _animationHandler;
        private Thread _animationThread;

        public RgbFusion(CancellationToken cancellationToken)
        {
            _commands = new AsyncQueue<LedCommand[]>(cancellationToken);
            this.cancellationToken = cancellationToken;
            _initEvent = new ManualResetEvent(false);
        }

        public bool IsInitialized()
        {
            return _ledFun != null && _initialized;
        }

        public void LoadProfile(int profileId)
        {
            _ledFun.Adv_mode_Apply(GetAllAreaInfo(profileId), GetAllExtAreaInfo(profileId));
            do
            {
                Thread.Sleep(10);
            }
            while (!_areaChangeApplySuccess);
        }

        private void FillAllAreaInfo()
        {
            _allAreaInfo = GetAllAreaInfo();
        }


        private List<CommUI.Area_class> GetAllAreaInfo(int profileId = -1)
        {
            if (profileId < 1)
            {
                profileId = _ledFun.Current_Profile;
            }
            var str = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\Pro", profileId.ToString(), ".xml");
            var color = CommUI.Int_To_Color((uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile(str, _ledFun.Get_MB_Area_number(), color, string.Concat("Profile ", profileId.ToString()));
            }
            return CommUI.Inport_from_xml(str, null);
        }

        public static void Creative_Profile(string proXmlFilePath, int areaCount, Color defaultColor, string profileName = "")
        {
            var lang = new Lang();
            var areaClasses = new List<CommUI.Area_class>();
            for (var i = 0; i < areaCount; i++)
            {
                if (areaCount != 8 && areaCount != 9)
                {
                    var patternCombItem = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                    patternCombItem.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(patternCombItem, i, null));
                }
                else if (i == 7 || i == 8)
                {
                    var mPatternInfo = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    mPatternInfo.Sel_Item.Background = mPatternInfo.Bg_Brush_Solid;
                    mPatternInfo.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    mPatternInfo.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(mPatternInfo.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(mPatternInfo, i, null));
                }
                else
                {
                    var bgBrushSolid = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    bgBrushSolid.Sel_Item.Background = bgBrushSolid.Bg_Brush_Solid;
                    bgBrushSolid.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    bgBrushSolid.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(bgBrushSolid.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(bgBrushSolid, i, null));
                }
            }
            CommUI.Export_to_xml(areaClasses, proXmlFilePath, profileName);
        }

        public static void Creative_Profile_Ext(string proXmlFilePath, List<Comm_LED_Fun.Ext_Led_class> extAreaInfo, Color defaultColor, string profileName = "")
        {
            var areaClasses = new List<CommUI.Area_class>();
            foreach (var areaInfo in extAreaInfo)
            {
                var patternCombItem = new CommUI.Pattern_Comb_Item
                {
                    Type = 0,
                    Bg_Brush_Solid = { Color = defaultColor },
                    Sel_Item = { Style = null }
                };
                patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                patternCombItem.Sel_Item.Content = string.Empty;
                patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                var areaClass = new CommUI.Area_class(patternCombItem, areaInfo.DivsNum, null)
                {
                    Ext_Area_id = areaInfo.extLDev
                };
                areaClasses.Add(areaClass);
            }
            CommUI.Export_to_xml(areaClasses, proXmlFilePath, profileName);
        }

        private void Fill_ExtArea_info()
        {
            _allExtAreaInfo = GetAllExtAreaInfo();
            _extAreaInfoLookup = _allExtAreaInfo.ToDictionary(x => x.Area_index, x => x);
        }

        private void Assign_ExtArea(CommUI.Area_class area)
        {
            if (_extAreaInfoLookup.ContainsKey(area.Area_index))
            {
                area.Ext_Area_id = _extAreaInfoLookup[area.Area_index].Ext_Area_id;
            }
        }

        private List<CommUI.Area_class> GetAllExtAreaInfo(int profileId = -1)
        {
            List<CommUI.Area_class> allExtAreaInfo;
            if (profileId < 1)
            {
                profileId = _ledFun.Current_Profile;
            }
            var str = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\ExtPro", profileId.ToString(), ".xml");
            var str1 = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\TempP.xml");
            var color = CommUI.Int_To_Color((uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile_Ext(str, _ledFun.LEd_Layout.Ext_Led_Array, color, string.Concat("ExProfile ", profileId.ToString()));
                allExtAreaInfo = CommUI.Inport_from_xml(str, null);
                return allExtAreaInfo;
            }
            Creative_Profile_Ext(str1, _ledFun.LEd_Layout.Ext_Led_Array, color, string.Concat("ExProfile ", profileId.ToString()));
            allExtAreaInfo = ReImport_ExtInfo(CommUI.Inport_from_xml(str, null), CommUI.Inport_from_xml(str1, null));
            File.Delete(str1);
            return allExtAreaInfo;
        }

        private static List<CommUI.Area_class> ReImport_ExtInfo(List<CommUI.Area_class> orgExtArea, IEnumerable<CommUI.Area_class> newExtArea)
        {
            var areaClasses = new List<CommUI.Area_class>();
            foreach (var area in newExtArea)
            {
                var num = 0;
                while (num < orgExtArea.Count)
                {
                    if (area.Ext_Area_id != orgExtArea[num].Ext_Area_id)
                    {
                        num++;
                    }
                    else
                    {
                        area.Pattern_info = new CommUI.Pattern_Comb_Item(orgExtArea[num].Pattern_info);
                        orgExtArea.RemoveAt(num);
                        break;
                    }
                }
                areaClasses.Add(area);
            }
            return areaClasses;
        }

        public void Init()
        {
            var initThread = new Thread(DoInit);
            initThread.SetApartmentState(ApartmentState.STA);
            initThread.Start();
            initThread.Join();
            _newChangeThread = new Thread(SetAreas);
            _newChangeThread.SetApartmentState(ApartmentState.STA);
            _newChangeThread.Start();
            _newChangeThread.Join();
            _animationThread.Join();
        }

        public void EnqueueCommands(LedCommand[] commands)
        {
            _commands.Enqueue(commands);
        }

        /*public void ChangeColorForAreas(List<LedCommand> commands)
        {
            _commands = commands;
            _commandEvent.Set();
            _commandEvent.Reset();
        }*/

        public void SetAllAreas(LedCommand obj)
        {
            var patternCombItem = new CommUI.Pattern_Comb_Item
            {
                Bg_Brush_Solid = { Color = obj.NewColor},
                Sel_Item = { Style = null }
            };

            patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
            patternCombItem.Sel_Item.Content = string.Empty;
            patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
            /*patternCombItem.But_Args[0].Scenes_type = 1;
            patternCombItem.But_Args[1].Scenes_type = 1;
            patternCombItem.But_Args[0].TransitionsTeime = 10;
            patternCombItem.But_Args[1].TransitionsTeime = 10;*/
            patternCombItem.Bri = obj.Bright;
            patternCombItem.Speed = obj.Speed;
            patternCombItem.Type = obj.NewMode;

            var allAreaInfo = _allAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, null)).ToList();

            var allExtAreaInfo = _allExtAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, null) { Ext_Area_id = areaInfo.Ext_Area_id }).ToList();

            allAreaInfo.AddRange(allExtAreaInfo);

            // Disable all animations running
            _animationHandler.Reset();       

            _ledFun.Set_Adv_mode(allAreaInfo, true);
            Thread.Sleep(_changeOperationDelay);
        }

        public void StartMusicMode()
        {
            _ledFun.Start_music_mode();
        }

        public void StopMusicMode()
        {
            _ledFun.Stop_music_mode();
        }

        public void SetAreas()
        {
            while (_newChangeThread.IsAlive && !cancellationToken.IsCancellationRequested)
            {                
                foreach (var commands in _commands.Messages)
                {
                    var areaInfo = new List<CommUI.Area_class>();

                    foreach (var command in commands)
                    {
                        // Parse special areas
                        // -1: All areas (all LEDs)
                        // -2: Music Mode
                        // -3: Profile
                        // -4: Animation Settings
                        // -5: Play Animations
                        // -6: Pause Animations
                        // -7: Stop Animations
                        if (command.AreaId == -1)
                        {
                            SetAllAreas(command);
                            continue;
                        }
                        else if (command.AreaId == -2)
                        {
                            if (command.NewMode == 1)
                            {
                                StartMusicMode();
                            }
                            else
                            {
                                StopMusicMode();
                            }

                            continue;
                        }
                        else if (command.AreaId == -3)
                        {
                            LoadProfile(command.NewMode);
                            continue;
                        }
                        else if (command.AreaId == -4)
                        {
                            if (command.Animation != null)
                            {
                                _animationHandler.AddAnimation(command.Animation);
                            }

                            continue;
                        }
                        else if (command.AreaId == -5)
                        {
                            _animationHandler.Play();
                            continue;
                        }
                        else if (command.AreaId == -6)
                        {
                            _animationHandler.Pause();
                            continue;
                        }
                        else if (command.AreaId == -7)
                        {
                            _animationHandler.Stop();
                            continue;
                        }
                        
                        var area = command.BuildArea();

                        // Remove any animations that may be running for this area
                        _animationHandler.RemoveAnimation((sbyte)area.Area_index);

                        areaInfo.Add(area);
                    }

                    ApplyAreas(areaInfo);
                }
            }
        }

        public void ApplyAreas(List<CommUI.Area_class> areas)
        {
            if (areas.Any())
            {
                // Assign any ExtArea ids as needed
                areas.ForEach(a => Assign_ExtArea(a));

                // Apply the lighting
                _ledFun.Set_Adv_mode(areas, true);                
            }
        }

        public string GetAreasReport()
        {
            var areasReport = string.Empty;
            if (_allAreaInfo.Count > 0)
            {
                areasReport += "Areas detected on " + _ledFun.Product_Name + Environment.NewLine;
                areasReport = _allAreaInfo.Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Area_index + Environment.NewLine));
            }
            areasReport += Environment.NewLine;

            if (_allExtAreaInfo.Count > 0)
                foreach (var areaInfo in _ledFun.LEd_Layout.Ext_Led_Array)
                {
                    var extDevice = _ledFun.Get_Ext_Led_Tip(areaInfo.extLDev);
                    areasReport += "Areas detected on " + extDevice + Environment.NewLine;
                    areasReport = _allExtAreaInfo.Where(area => area.Ext_Area_id == areaInfo.extLDev).Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Area_index + Environment.NewLine));
                }
            areasReport += Environment.NewLine;
            areasReport += "Use Area ID -1 to set all areas at the same time.";

            return areasReport;
        }

        private void CallBackLedFunApplyScanPeripheralSuccess()
        {             
            _initEvent.Set();
        }
        private void CallBackLedFunApplyEzSuccess() => _areaChangeApplySuccess = true;
        private void CallBackLedFunApplyAdvSuccess() => _areaChangeApplySuccess = true;

        private void DoInit()
        {
            if (_ledFun != null)
                return;

            _ledFun = new Comm_LED_Fun(false);
            _ledFun.Apply_ScanPeriphera_Scuuess += CallBackLedFunApplyScanPeripheralSuccess;
            _ledFun.ApplyEZ_Success += CallBackLedFunApplyEzSuccess;
            _ledFun.ApplyAdv_Success += CallBackLedFunApplyAdvSuccess;

            _ledFun.Ini_LED_Fun();
            _ledFun.Ini_DDR_Info();

            _ledFun = CommUI.Get_Easy_Pattern_color_Key(_ledFun);

            _ledFun.LEd_Layout.Set_Support_Flag();

            switch(WaitHandle.WaitAny(new WaitHandle[] { cancellationToken.WaitHandle, _initEvent }))
            {
                // Shutdown was called during init, so stop
                case 0:
                    return;

                default:
                    break;
            }            

            _ledFun.Current_Mode = 0; // 1= Advanced 0 = Simple or Ez

            _ledFun.Led_Ezsetup_Obj.PoweronStatus = 1;
            StopMusicMode();
            _initialized = true;
            _ledFun.Set_Sync(false);
            StopMusicMode();
            FillAllAreaInfo();
            Fill_ExtArea_info();

            // Setup the animation handler
            _animationHandler = new AnimationHandler(cancellationToken, ApplyAreas);
            _animationThread = new Thread(_animationHandler.AnimationLoop);
            _animationThread.SetApartmentState(ApartmentState.STA);            
            _animationThread.Start();
        }
    }
}