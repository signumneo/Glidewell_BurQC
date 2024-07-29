using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static User_Interface.Program;
using QCBur_dll;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using User_Interface.Main;
using Microsoft.Office.Core;
using System.Diagnostics;
using User_Interface.Cloud;
using static User_Interface.Properties.Settings;
using static User_Interface.SetupService;
using Newtonsoft.Json.Linq;

namespace User_Interface
{
    public partial class Sorting : UserControl
    {
        CSVFunctions csvFunctions = new CSVFunctions();
        Stopwatch stopwatch = new Stopwatch();
        GF globalFunctions = new GF();
        const int GC = 0;
        const int MTP = 1;
        const int BUR_RESULT = 2;
        const int BAD_SCAN = 3;
        const int DISCARD = 4;
        const int CSV_FILE_IDX = 41;
        public Sorting()
        {
            InitializeComponent();
        }

        private void Sorting_Load(object sender, EventArgs e)
        {
            try
            {
                string grainCoverageCriteria1String = mPConfig.GrainCoverage.GC1.ToString();
                string grainCoverageCriteria2String = mPConfig.GrainCoverage.GC2.ToString();
                string grainCoverageCriteria3String = mPConfig.GrainCoverage.GC3.ToString();
                string grainCoverageCriteria4String = mPConfig.GrainCoverage.GC4.ToString();
                string grainCoverageCriteria5String = mPConfig.GrainCoverage.GC5.ToString();
                string meanGrainProtusionCriteria1String = mPConfig.MeanGrainProtustion.MTV1.ToString();

                lblGrainCoverageCriteria1.Text = grainCoverageCriteria1String;
                lblGrainCoverageCriteria2.Text = grainCoverageCriteria2String;
                lblGrainCoverageCriteria3.Text = grainCoverageCriteria3String;
                lblGrainCoverageCriteria4.Text = grainCoverageCriteria4String;
                lblGrainCoverageCriteria5.Text = grainCoverageCriteria5String;
                lblMeanGrainProtusionCriteria1.Text = meanGrainProtusionCriteria1String;

                //Parse criteria values to integers 
                //Int32.TryParse(grainCoverageCriteria1String, out grainCoverageCriteria1);
                //Int32.TryParse(grainCoverageCriteria2String, out grainCoverageCriteria2);
                //Int32.TryParse(grainCoverageCriteria3String, out grainCoverageCriteria3);
                //Int32.TryParse(grainCoverageCriteria4String, out grainCoverageCriteria4);
                //Int32.TryParse(grainCoverageCriteria5String, out grainCoverageCriteria5);
                //Int32.TryParse(meanGrainProtusionCriteria1String, out meanGrainProtusionCriteria1);
                Btn_sortingFinished.Enabled = false;
                Btn_notSorted.Enabled = false;

                lbl_assemblyVersion.Text = GF.GetThreeDigitsVersionNumber();
            }
            catch (Exception ex)
            {

            }
        }

        private void Btn_getSortingInfo_Click(object sender, EventArgs e)
        {
            
            if (String.IsNullOrEmpty(TB_fixtureBarcode.Text))
            {
                MessageBox.Show("Please scan the fixture barcode");
            }
            else
            {
                try
                {
                    string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
                    stopwatch.Restart();
                    TB_fixtureBarcode.Enabled = false;
                    Dictionary<int, string> converted = null;
                    Dictionary<int, string> returnedInfoSDK = null;
                    string returnedInfoLambda;
                    #region aws sdk
                    if (!Default.IsUsingHttp)
                    {
                        converted = dbClient.GetFixtureInfo(fixtureId, Databases.FIXTURE_DATABASE);
                    }
                    else
                    {
                        returnedInfoLambda = mCloud.GetSortingInformation(fixtureId);
                        if (returnedInfoLambda != "{}")
                            converted = JsonConvert.DeserializeObject<Dictionary<int, string>>(returnedInfoLambda);
                    }

                    if (converted != null)
                    {
                        if (!string.IsNullOrEmpty(converted[CSV_FILE_IDX]))
                        {
                            TB_lotFileName.Text = converted[CSV_FILE_IDX];
                        }
                        converted.Remove(CSV_FILE_IDX);
                        //var sort = JsonConvert.DeserializeObject<Sorting>(sortingInfo);
                        if (converted != null)
                        {

                            //globalFunctions.RetrievePassFailCriteria();
                            foreach (int key in converted.Keys)
                            {
                                string buttonName = "btn_Bur" + (key+1).ToString();
                                string mtvLabelName = "lbl_MTVBur" + (key+1).ToString();
                                string cgLabelName = "lbl_GCBur" + (key+1).ToString();
                                // {GC, MTP, BurResult, BadScan, Discard}
                                string[] burData = converted[key].Split(',');
                                if (burData[BAD_SCAN].ToUpper() == "TRUE")
                                {
                                    panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                                    panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                                    continue;
                                }
                                else if (burData[DISCARD].ToUpper() == "TRUE")
                                {
                                    panel_Bur_Buttons.Controls[mtvLabelName].Text = "DISCARD";
                                    panel_Bur_Buttons.Controls[cgLabelName].Text = "DISCARD";
                                    continue;
                                }

                                panel_Bur_Buttons.Controls[mtvLabelName].Text = burData[MTP].ToString();
                                panel_Bur_Buttons.Controls[cgLabelName].Text = burData[GC].ToString();
                                if (burData[BUR_RESULT] == "BNOW")
                                {
                                    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.MediumSeaGreen;
                                    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.MediumSeaGreen;
                                    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                                }

                                else if (burData[BUR_RESULT] == "BENA")
                                {
                                    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.DodgerBlue;
                                    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.DodgerBlue;
                                    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;
                                }

                                else if (burData[BUR_RESULT] == "Quarantine")
                                {
                                    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Orange;
                                    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Orange;
                                    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.MediumSeaGreen;

                                }
                                else if (burData[BUR_RESULT] == "Fail")
                                {
                                    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Brown;
                                    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Brown;
                                    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Brown;
                                }
                                else if (burData[BUR_RESULT] == "Undetermined")
                                {
                                    panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
                                    panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";
                                    panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                                    panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";
                                    panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                                }
                            }
                            Btn_notSorted.Enabled = true;
                            Btn_sortingFinished.Enabled = true;
                            Btn_sortingFinished.Enabled = true;
                        } 
                    }
                    else
                    {
                        MessageBox.Show($"Data for fixture {fixtureId} not found");

                        ClearAll();
                    }
                    #endregion
                    
                    stopwatch.Stop();
                    globalFunctions.LogThis($"GetSortingInformation for fixture id {fixtureId} took {stopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to get sorting information: " + ex.Message);
                    globalFunctions.LogThis("Failed to get sorting information: " + ex.ToString());
                }
            }

        }

        private void Btn_sortingFinished_Click(object sender, EventArgs e)
        {
            try
            {
                string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
                DialogResult result = MessageBox.Show($"Are you done sorting the burs? If you click \'Yes,\' the current bur information associated with fixture {fixtureId} will be deleted from the database", "Confirm Operation", MessageBoxButtons.YesNo);
                bool updated = false;
                int numTries = 0;
                switch (result)
                {
                    case DialogResult.Yes:
                        if (!Default.IsUsingHttp)
                        {
                            do
                            {
                                do
                                {
                                    updated = dbClient.UpdateSortedItem(fixtureId, TB_lotFileName.Text, true, false);
                                    if (!updated) numTries++;
                                }
                                while (!updated && numTries < GF.MAX_FAILURES);
                                if (updated)
                                {
                                    globalFunctions.LogThis($"Successful operation: Burs in fixture {fixtureId} were moved to the individual bur database");
                                    MessageBox.Show("SortingFinished operation finished!");

                                }
                                else if (numTries >= GF.MAX_FAILURES)
                                {
                                    globalFunctions.LogThis($"Operation failed: Burs in fixture {fixtureId} were not moved to the individual bur database");
                                    MessageBox.Show("ERROR: burs were not moved to the individual bur database");
                                } 
                            } while (!updated &&
                             DialogResult.Retry == MessageBox.Show("BUR UPDATE FAILED: \nTo try again, press \'Retry\'. Otherwise, click \'Cancel\'", "SORTING FINISHED OPERATION FAILED!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error));

                        }

                        else mCloud.UpdateSortedInformation(fixtureId, TB_lotFileName.Text, true);
                        if (updated) ClearAll();
                        break;
                    case DialogResult.No:
                        MessageBox.Show($"\'No\' Selected: No action will be taken");
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("SortingFinished operation failed: " + ex.Message);
                globalFunctions.LogThis("SortingFinished operation failed: " + ex.ToString());
            }
        }

        private void ClearAll()
        {
            TB_fixtureBarcode.Clear();
            TB_fixtureBarcode.Enabled = true;
            Btn_notSorted.Enabled = false;
            Btn_sortingFinished.Enabled = false;
            Btn_getSortingInfo.Enabled = true;
            TB_lotFileName.Clear();

            for (int i = 0; i < 40; i++)
            {
                string buttonName = "btn_Bur" + (i + 1).ToString();
                string mtvLabelName = "lbl_MTVBur" + (i + 1).ToString();
                string cgLabelName = "lbl_GCBur" + (i + 1).ToString();

                panel_Bur_Buttons.Controls[cgLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[cgLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[mtvLabelName].ForeColor = Color.Gray;
                panel_Bur_Buttons.Controls[mtvLabelName].Text = "n/a";

                panel_Bur_Buttons.Controls[buttonName].BackColor = Color.Silver;
            }
        }

        private void Btn_notSorted_Click(object sender, EventArgs e)
        {
            try
            {
                string fixtureId = TB_fixtureBarcode.Text.Replace(" ", "");
                DialogResult result = MessageBox.Show("Are the burs not going to be sorted? If you click \'Yes,\' the bur information will be deleted from the database forever", "Confirm Operation", MessageBoxButtons.YesNo);
                bool updated = false;
                int numTries = 0;
                switch (result)
                {
                    case DialogResult.Yes:
                        do
                        {
                            if (!Default.IsUsingHttp)
                            {
                                do
                                {
                                    updated = dbClient.UpdateSortedItem(fixtureId, TB_lotFileName.Text, false, false);
                                    if (!updated) numTries++;
                                }
                                while (!updated && numTries < GF.MAX_FAILURES);
                                //updated = false;
                                //numTries = 3;

                            }
                            else mCloud.UpdateSortedInformation(fixtureId, TB_lotFileName.Text, false);
                            if (updated)
                            {
                                globalFunctions.LogThis($"Successful operation: Burs in fixture {fixtureId} were deleted from the database");
                                MessageBox.Show("NotSorted operation finished!");
                            }
                            else if (numTries >= GF.MAX_FAILURES)
                            {
                                globalFunctions.LogThis($"Operation failed: Burs in fixture {fixtureId} were not deleted from the database");
                                MessageBox.Show("ERROR: information deletion failed");
                            } 
                        } while (!updated &&
                             DialogResult.Retry == MessageBox.Show("BUR UPDATE FAILED: \nTo try again, press \'Retry\'. Otherwise, click \'Cancel\'", "NOT SORTED OPERATION FAILED!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error));


                        if (updated) ClearAll();
                        break;
                    case DialogResult.No:
                        MessageBox.Show($"\'No\' Selected: No action will be taken");
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("NotSorted operation failed: " + ex.Message);
                globalFunctions.LogThis("NotSorted operation failed: " + ex.ToString());
            }
        }

        private void Btn_clear_Click(object sender, EventArgs e)
        {
            ClearAll();
        }
    }
}
