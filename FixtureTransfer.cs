/*****************************************************************************************
 *                                                                                        *
 *                                  !!! Code Notes !!!                                    *
 *                                                                                        *
 *                                Bidirectional Transfer UI                               *
 *                                                                                        *
 * This C# application facilitates the transfer of dental burs between an Acurata Box and *
 * Alicona Fixtures based on user direction selection. It integrates with AWS services    *
 * for data storage and retrieval, ensuring seamless and accurate bidirectional transfer. *
 *                                                                                        *
 * Application Functional Description:                                                    *
 * 1. **User Interface Initialization:** Initializes buttons, textboxes, and other UI     *
 *    elements.                                                                           *
 * 2. **Direction Selection:** Users select the transfer direction using a ComboBox.      *
 *    - "->": Forward Transfer (Box to Fixture)                                           *
 *    - "<-": Reverse Transfer (Fixture to Box)                                           *
 * 3. **Grid Loading:** Based on the selected direction, prompts user to load either the  *
 *    box or fixture grid first using barcode scanners.                                   *
 * 4. **Data Transfer:** Handles the transfer of burs with one-to-one correspondence      *
 *    between slots in the box and fixture.                                               *
 * 5. **AWS Integration:** Utilizes AWS Lambda and DynamoDB for backend processing and    *
 *    data management.                                                                    *
 * 6. **UI Update:** Ensures the UI accurately reflects the current state after each      *
 *    transfer.                                                                           *
 * 7. **Event Handlers:** Includes various event handlers for button clicks, text changes,*
 *    and combobox interactions.     
 *****************************************************************************************/

// Import statements
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.Lambda.Model;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using QCBur_dll;
using Amazon.Runtime;
using System.Linq;
using System.Text;

// All functionalities related to the fixture transfer are implemented under this class
namespace User_Interface
{
    public partial class FixtureTransfer : UserControl
    {

        // Initialize the components and get everything set for the application to run



        // Define the Lambda Client 
        private AWSSimpleLambda lambdaClient;

        // Define the necessary UI elements
        private List<Button> boxButtons;
        private List<Button> fixtureButtons;
        public string boxBarcode;
        private bool isComboBoxHandled = false;
        private string fixtureID;
        private string trayID;
        private string lastInHouseText = string.Empty;
        private string lastAliconaText = string.Empty;
        private System.Windows.Forms.Timer scanTimer;
        private System.Windows.Forms.Timer flashMessageTimer;
        private string lastScannedText = string.Empty;
        private int currentBurIndex = 0;
        private StringBuilder scanBuffer = new StringBuilder();
        private System.Windows.Forms.Timer debounceTimer;

        public FixtureTransfer()
        {
            InitializeComponent();
            InitializeButtons();

            // Add items to the ComboBox
            comboBox1.Items.Add("->");
            comboBox1.Items.Add("<-");

            // Attach event handlers
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox1.MouseHover += comboBox1_MouseHover;
            comboBox1.DrawItem += comboBox1_DrawItem;
            comboBox1.DropDownClosed += comboBox1_DropDownClosed;

            // Set DrawMode to OwnerDrawFixed to customize tooltips for items
            comboBox1.DrawMode = DrawMode.OwnerDrawFixed;

            // Disable text boxes initially
            TB_inHouseFixture.Enabled = false;
            TB_aliconaFixture.Enabled = false;

            // Initialize the ToolTip
            toolTip = new ToolTip();
            // toolTip.SetToolTip(acurataScanner, "Please select the transfer direction first.");
            // toolTip.SetToolTip(aliconaScanner, "Please select the transfer direction first.");

            // Attach KeyDown event handlers
            TB_inHouseFixture.KeyDown += TB_inHouseFixture_KeyDown;
            TB_aliconaFixture.KeyDown += TB_aliconaFixture_KeyDown;

            // Attach KeyDown event handlers
            TB_inHouseFixture.KeyDown += TB_inHouseFixture_KeyDown;
            TB_aliconaFixture.KeyDown += TB_aliconaFixture_KeyDown;

            // Initialize label3
            label3 = new Label();
            label3.Name = "label3";
            label3.Size = new Size(200, 200); // Adjust size as needed
            label3.Location = new Point(780, 303); // Adjust position as needed
            label3.Font = new Font("Arial", 16, FontStyle.Bold); // Set font size and style
            label3.ForeColor = Color.Black; // Set text color
            label3.BackColor = Color.Transparent; // Set background color
            label3.Visible = false; // Initially set to false

            // Add label3 to the form
            this.Controls.Add(label3);

            button1.Click += new EventHandler(button1_Click);
            button2.Click += new EventHandler(button2_Click);
        }

        // Essential to populate the buttons in the UI properly and to ensure the order is correct
        private void InitializeButtons()
        {
            boxButtons = new List<Button>();
            fixtureButtons = new List<Button>();

            // Add all box buttons to the list (panel3 contains the box buttons)
            foreach (Control control in panel3.Controls)
            {
                if (control is Button button)
                {
                    boxButtons.Add(button);
                    Console.WriteLine($"Added button {button.Text} to boxButtons.");
                }
            }

            // Add all fixture buttons to the list (panel_Bur_Buttons contains the fixture buttons)
            foreach (Control control in panel_Bur_Buttons.Controls)
            {
                if (control is Button button)
                {
                    fixtureButtons.Add(button);
                    Console.WriteLine($"Added button {button.Text} to fixtureButtons.");
                }
            }

            // Sort the boxButtons and fixtureButtons to ensure the order is correct
            boxButtons = boxButtons.OrderBy(b => int.Parse(b.Text)).ToList();
            fixtureButtons = fixtureButtons.OrderBy(b => int.Parse(b.Text)).ToList();
        }

        // Tooltip instance
        ToolTip toolTip = new ToolTip();

        // Event handler for when the user hovers over the ComboBox
        private void comboBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip.SetToolTip(comboBox1, "Choose transfer direction");
        }

        // Event handler for when an item is selected in the ComboBox
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isComboBoxHandled) return; // Prevent re-entry

            try
            {
                isComboBoxHandled = true; // Set flag to prevent re-entry
                string selectedItem = comboBox1.SelectedItem.ToString();

                // Check the selected direction and enable/disable textboxes accordingly
                if (selectedItem == "->")
                {
                    TB_inHouseFixture.Enabled = true;
                    TB_aliconaFixture.Enabled = false;

                    // Update tooltips
                    // toolTip.SetToolTip(acurataScanner, "Click to scan the Acurata Box barcode.");
                    // toolTip.SetToolTip(aliconaScanner, "Disabled for forward transfer.");
                }
                else if (selectedItem == "<-")
                {
                    TB_inHouseFixture.Enabled = false;
                    TB_aliconaFixture.Enabled = true;

                    // Update tooltips
                    // toolTip.SetToolTip(aliconaScanner, "Click to scan the Alicona Fixture barcode.");
                    // toolTip.SetToolTip(acurataScanner, "Disabled for reverse transfer.");
                }
            }
            finally
            {
                isComboBoxHandled = false; // Reset flag after handling
            }
        }

        // Event handler to draw items in the ComboBox to show tooltips
        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            // Draw the background of the item.
            e.DrawBackground();

            // Get the item text
            string itemText = comboBox1.Items[e.Index].ToString();

            // Determine the tooltip text based on the item
            string toolTipText = itemText == "->" ? "Forward transfer" : "Reverse transfer";

            // Draw the item text
            e.Graphics.DrawString(itemText, e.Font, Brushes.Black, e.Bounds);

            // Set the tooltip for the current item
            toolTip.SetToolTip(comboBox1, toolTipText);

            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();
        }

        // Event handler to reset the tooltip when the dropdown is closed
        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            toolTip.SetToolTip(comboBox1, "Choose transfer direction");
        }

        // This method is used to clear the screen and reset the UI
        private void btnClearScreen_Click(object sender, EventArgs e)
        {
            ResetUI();
        }

        private void ResetUI()
        {
            // Show confirmation message to the user
            var result = MessageBox.Show("Do you want to reset the UI?", "Confirm Reset", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                // Reset box buttons to Silver
                foreach (var button in boxButtons)
                {
                    button.BackColor = Color.Silver;
                }

                // Reset fixture buttons to Silver
                foreach (var button in fixtureButtons)
                {
                    button.BackColor = Color.Silver;
                }

                // Clear the textboxes
                TB_inHouseFixture.Clear();
                TB_aliconaFixture.Clear();
                textBox1.Clear();

                // Reset the label and panel states
                label3.Text = string.Empty;
                label3.Visible = false;
                panel_Bur_Buttons.Visible = true;

                currentBurIndex = 0;
                Console.WriteLine("UI reset.");
            }
            else
            {
                Console.WriteLine("UI reset cancelled.");
            }
        }
        // Enable other textbox based on transfer direction
        private void EnableOtherTextbox(string direction)
        {
            if (direction == "->")
            {
                TB_aliconaFixture.Enabled = true;
            }
            else if (direction == "<-")
            {
                TB_inHouseFixture.Enabled = true;
            }
        }


        private void TB_inHouseFixture_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents the beep sound on Enter key
                _ = ProcessScannedBoxBarcode(TB_inHouseFixture.Text); // Call async method without awaiting
            }
        }

        // Button click handler for button1 (Box barcode)
        private async void button1_Click(object sender, EventArgs e)
        {
            string boxBarcode = TB_inHouseFixture.Text;
            if (!string.IsNullOrEmpty(boxBarcode))
            {
                await ProcessScannedBoxBarcode(boxBarcode);
                EnableOtherTextbox(comboBox1.SelectedItem.ToString());
            }
        }


        private void TB_aliconaFixture_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevents the beep sound on Enter key
                _ = ProcessScannedFixtureID(TB_aliconaFixture.Text); // Call async method without awaiting
            }
        }

        // Button click handler for button2 (Fixture barcode)
        private async void button2_Click(object sender, EventArgs e)
        {
            string fixtureID = TB_aliconaFixture.Text;
            if (!string.IsNullOrEmpty(fixtureID))
            {
                await ProcessScannedFixtureID(fixtureID);
                EnableOtherTextbox(comboBox1.SelectedItem.ToString());
            }
        }

        private async Task ProcessScannedBoxBarcode(string boxBarcode)
        {
            Console.WriteLine($"Scanned boxBarcode: {boxBarcode}");

            if (string.IsNullOrEmpty(boxBarcode) || boxBarcode.Length <= 7) // Adjust length check as needed
            {
                Console.WriteLine("No boxBarcode scanned or incomplete barcode.");
                return;
            }

            // Check if the boxBarcode exists in the database
            bool boxExists = await CheckBoxExistsAsync(boxBarcode);
            if (!boxExists)
            {
                Console.WriteLine($"Box with barcode {boxBarcode} does not exist in the database.");
                LoadEmptyBoxGrid(); // Load empty box grid if the box does not exist
                return;
            }

            string selectedDirection = comboBox1.SelectedItem.ToString();
            bool isForwardTransfer = selectedDirection == "->";

            if (!isForwardTransfer)
            {
                // Check if the box is almost full for reverse transfer
                bool isBoxAlmostFull = await CheckBoxFullStatusAsync(boxBarcode);
                if (isBoxAlmostFull)
                {
                    var result = MessageBox.Show("The selected box has more than 85 slots filled. Do you still want to load the box?", "Confirm Load", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        Console.WriteLine("Loading box cancelled by the user.");
                        return;
                    }
                }
            }

            // Load box grid data
            await PopulateBoxSlotsAsync(boxBarcode);

            // Enable the Alicona text box for forward transfer
            TB_aliconaFixture.Enabled = true;
            TB_inHouseFixture.Enabled = true;
        }

        private async Task ProcessScannedFixtureID(string fixtureID)
        {
            Console.WriteLine($"Scanned fixtureID: {fixtureID}");

            if (string.IsNullOrEmpty(fixtureID)) // Adjust length check as needed
            {
                Console.WriteLine("No fixtureID scanned or incomplete barcode.");
                return;
            }

            textBox1.Text = fixtureID;

            string selectedDirection = comboBox1.SelectedItem.ToString();
            bool isForwardTransfer = selectedDirection == "->";

            // Check if the fixtureID exists in the database
            bool fixtureExists = await CheckFixtureExistsAsync(fixtureID);
            if (!fixtureExists)
            {
                Console.WriteLine($"Fixture with ID {fixtureID} does not exist in the database.");
                LoadEmptyFixtureGrid(); // Load empty fixture grid if the fixture does not exist
                return;
            }

            // Populate fixture slots with bur data if fixture is not occupied or it's a reverse transfer
            await PopulateFixtureSlotsAsync(fixtureID, isForwardTransfer);
            Console.WriteLine("Fixture slots populated.");
            Console.WriteLine($"Selected fixtureID: {fixtureID}");

            // Enable the Acurata text box for reverse transfer
            TB_inHouseFixture.Enabled = true;
            TB_aliconaFixture.Enabled = true;
        }

        // Method to check if boxBarcode exists in the database
        private async Task<bool> CheckBoxExistsAsync(string boxBarcode)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurBoxTable_TEST");
            var filter = new QueryFilter("boxBarcode", QueryOperator.Equal, boxBarcode);
            var search = table.Query(filter);
            var results = await search.GetNextSetAsync();
            return results.Count > 0;
        }

        // Method to create a new entry for boxBarcode in the database
        private async Task CreateNewBoxEntryAsync(string boxBarcode)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurBoxTable_TEST");
            var item = new Document();
            item["boxBarcode"] = boxBarcode;
            // Add any other necessary fields and their default values
            await table.PutItemAsync(item);
        }

        // Method to check if fixtureID exists in the database
        private async Task<bool> CheckFixtureExistsAsync(string fixtureID)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurQCTrayTable");
            var filter = new QueryFilter("trayId", QueryOperator.Equal, fixtureID);
            var search = table.Query(filter);
            var results = await search.GetNextSetAsync();
            return results.Count > 0;
        }

        // Method to create a new entry for fixtureID in the database
        private async Task CreateNewFixtureEntryAsync(string fixtureID)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurQCTrayTable");
            var item = new Document();
            item["trayId"] = fixtureID;
            // Add any other necessary fields and their default values
            await table.PutItemAsync(item);
        }

        // UI Loading logic for bidirectional transfer is here

        // Forward Transfer: Box -> Fixture

        // This method is used to refresh the box grid based on the current state of the boxBarcode key in the database
        private async Task PopulateBoxSlotsAsync(string boxBarcode)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurBoxTable_TEST");
            var filter = new QueryFilter("boxBarcode", QueryOperator.Equal, boxBarcode);
            var search = table.Query(filter);

            try
            {
                var documentSet = await search.GetNextSetAsync();

                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];
                    for (int i = 1; i <= 100; i++)
                    {
                        string burKey = $"bur{i}";
                        if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                        {
                            var burData = burEntry.AsDocument();
                            if (burData != null && burData.Count > 0)
                            {
                                // Check the values of P1 and P2
                                string p1 = burData.TryGetValue("P1", out DynamoDBEntry p1Entry) ? p1Entry.AsString() : "0";
                                string p2 = burData.TryGetValue("P2", out DynamoDBEntry p2Entry) ? p2Entry.AsString() : "0";

                                if (p1 != "0" && p2 != "0")
                                {
                                    boxButtons[i - 1].BackColor = Color.DeepSkyBlue; // Assuming DeepSkyBlue indicates filled
                                }
                                else
                                {
                                    boxButtons[i - 1].BackColor = Color.Silver; // Assuming Silver indicates empty
                                }
                            }
                            else
                            {
                                boxButtons[i - 1].BackColor = Color.Silver; // Assuming Silver indicates empty
                            }
                        }
                        else
                        {
                            boxButtons[i - 1].BackColor = Color.Silver; // Assuming Silver indicates empty
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing box grid: {ex.Message}");
                Logger.LogError("Error refreshing box grid", ex);
            }
        }


        // This method is used to check if all box slots are filled
        private bool IsBoxFilled()
        {
            foreach (var button in boxButtons)
            {
                if (button.BackColor != Color.DeepSkyBlue)
                {
                    return false;
                }
            }
            return true;
        }

        // Reverse Transfer: Fixture -> Box

        private async Task<bool> CheckBoxFullStatusAsync(string boxBarcode)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurBoxTable_TEST");
            var filter = new QueryFilter("boxBarcode", QueryOperator.Equal, boxBarcode);
            var search = table.Query(filter);

            try
            {
                Console.WriteLine($"Checking fill status for boxBarcode: {boxBarcode}");
                var documentSet = await search.GetNextSetAsync();

                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];
                    Console.WriteLine($"Document for boxBarcode {boxBarcode}: {JsonConvert.SerializeObject(document)}");

                    int filledSlots = 0;
                    for (int i = 1; i <= 100; i++)
                    {
                        string burKey = $"bur{i}";
                        if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                        {
                            var burData = burEntry.AsDocument();
                            if (burData != null && burData.Count > 0)
                            {
                                filledSlots++;
                            }
                        }
                    }

                    Console.WriteLine($"Box {boxBarcode} has {filledSlots} filled slots.");
                    return filledSlots > 85;
                }
                else
                {
                    Console.WriteLine($"No items found for boxBarcode: {boxBarcode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking box fill status: {ex.Message}");
                MessageBox.Show("Error checking box fill status.");
                Logger.LogError("Error checking box fill status", ex);
                return false;
            }
        }

        private async Task PopulateFixtureSlotsAsync(string selectedFixtureID, bool isForwardTransfer, bool showOccupiedMessage = true)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurQCTrayTable");
            var filter = new QueryFilter("trayId", QueryOperator.Equal, selectedFixtureID);
            var search = table.Query(filter);

            try
            {
                Console.WriteLine($"Querying fixture data for fixtureID: {selectedFixtureID}");
                var documentSet = await search.GetNextSetAsync();

                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];
                    Console.WriteLine($"Document for fixtureID {selectedFixtureID}: {JsonConvert.SerializeObject(document)}");

                    bool hasBurs = false;
                    for (int i = 1; i <= 100; i++)
                    {
                        string[] burKeys = { $"testbur{i}", $"bur{i}" };
                        foreach (var burKey in burKeys)
                        {
                            if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                            {
                                var burData = burEntry.AsString(); // Get the JSON string
                                if (!string.IsNullOrEmpty(burData))
                                {
                                    hasBurs = true;
                                    break;
                                }
                            }
                        }
                        if (hasBurs)
                        {
                            break;
                        }
                    }

                    if (isForwardTransfer)
                    {
                        if (hasBurs && showOccupiedMessage)
                        {
                            // Show "Fixture Occupied" message and hide the grid
                            label3.Text = "Fixture Occupied";
                            label3.Visible = true;
                            label3.BringToFront();
                            panel_Bur_Buttons.Visible = false; // Assuming panel_Bur_Buttons contains the fixture buttons
                            Console.WriteLine($"Fixture {selectedFixtureID} has burs.");
                        }
                        else
                        {
                            // Hide the "Fixture Occupied" message and show the grid
                            label3.Visible = false;
                            panel_Bur_Buttons.Visible = true;

                            // Populate the TB_aliconaFixture and textBox1 with the fixtureID
                            TB_aliconaFixture.Text = selectedFixtureID; // Assuming fixtureID for now is of the form FX004
                            textBox1.Text = selectedFixtureID;
                            Console.WriteLine($"Populated fixtureID: {selectedFixtureID} into TB_aliconaFixture and textBox1.");

                            // Clear all fixture buttons to Silver
                            foreach (var button in fixtureButtons)
                            {
                                button.BackColor = Color.Silver;
                            }

                            // Iterate over all possible slots (1 to 100) and update UI accordingly
                            for (int i = 1; i <= 100; i++)
                            {
                                string[] burKeys = { $"testbur{i}", $"bur{i}" };
                                foreach (var burKey in burKeys)
                                {
                                    if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                                    {
                                        var burData = burEntry.AsString(); // Get the JSON string
                                        if (!string.IsNullOrEmpty(burData))
                                        {
                                            var burJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(burData);
                                            if (burJson != null && burJson.Count > 0)
                                            {
                                                // Check if the slot is within the range of fixture buttons
                                                if (i <= fixtureButtons.Count)
                                                {
                                                    fixtureButtons[i - 1].BackColor = Color.DeepSkyBlue; // Assuming DeepSkyBlue indicates filled
                                                    Console.WriteLine($"Loaded {burKey} into fixture slot {i}.");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Slot {i}: {burKey} is empty.");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Reverse transfer: Populate slots regardless of burs presence

                        // Hide the "Fixture Occupied" message and show the grid
                        label3.Visible = false;
                        panel_Bur_Buttons.Visible = true;

                        // Populate the TB_aliconaFixture and textBox1 with the fixtureID
                        TB_aliconaFixture.Text = selectedFixtureID; // Assuming fixtureID for now is of the form FX004
                        textBox1.Text = selectedFixtureID;
                        Console.WriteLine($"Populated fixtureID: {selectedFixtureID} into TB_aliconaFixture and textBox1.");

                        // Clear all fixture buttons to Silver
                        foreach (var button in fixtureButtons)
                        {
                            button.BackColor = Color.Silver;
                        }

                        // Iterate over all possible slots (1 to 100) and update UI accordingly
                        for (int i = 1; i <= 100; i++)
                        {
                            string[] burKeys = { $"testbur{i}", $"bur{i}" };
                            foreach (var burKey in burKeys)
                            {
                                if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                                {
                                    var burData = burEntry.AsString(); // Get the JSON string
                                    if (!string.IsNullOrEmpty(burData))
                                    {
                                        var burJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(burData);
                                        if (burJson != null && burJson.Count > 0)
                                        {
                                            // Check if P1 and P2 have values that are not 0 or empty
                                            if (burJson.ContainsKey("P1") && burJson.ContainsKey("P2") &&
                                                !string.IsNullOrEmpty(burJson["P1"].ToString()) && burJson["P1"].ToString() != "0" &&
                                                !string.IsNullOrEmpty(burJson["P2"].ToString()) && burJson["P2"].ToString() != "0")
                                            {
                                                // Check if the slot is within the range of fixture buttons
                                                if (i <= fixtureButtons.Count)
                                                {
                                                    fixtureButtons[i - 1].BackColor = Color.DeepSkyBlue; // Assuming DeepSkyBlue indicates filled
                                                    Console.WriteLine($"Loaded {burKey} into fixture slot {i}.");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Slot {i}: {burKey} is empty.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"No items found for fixtureID: {selectedFixtureID}");
                    LoadEmptyFixtureGrid();
                    label3.Text = "No associated fixture data found. Empty grid loaded.";
                    label3.Visible = true;
                    label3.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No bur data: {ex.Message}");
                Logger.LogError("Error populating fixture slots", ex);
                LoadEmptyFixtureGrid();
            }
        }


        private void LoadEmptyBoxGrid()
        {
            for (int i = 0; i < boxButtons.Count; i++)
            {
                boxButtons[i].BackColor = Color.Silver; // Assuming Silver indicates empty
            }
            Console.WriteLine("Loaded empty box grid.");
        }

        private void LoadEmptyFixtureGrid()
        {
            for (int i = 0; i < fixtureButtons.Count; i++)
            {
                fixtureButtons[i].BackColor = Color.Silver; // Assuming Silver indicates empty
            }
            Console.WriteLine("Loaded empty fixture grid.");
        }

        private async void btnTransfer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TB_inHouseFixture.Text))
            {
                Console.WriteLine("No box scanned.");
                return;
            }
            if (string.IsNullOrEmpty(TB_aliconaFixture.Text))
            {
                Console.WriteLine("No fixtureID scanned.");
                return;
            }

            Console.WriteLine("Transfer started.");

            string fixtureID = TB_aliconaFixture.Text;
            string boxBarcode = TB_inHouseFixture.Text;

            Console.WriteLine($"Scanned fixtureID: {fixtureID}");
            Console.WriteLine($"Scanned boxBarcode: {boxBarcode}");

            // Determine the direction of transfer
            string selectedDirection = comboBox1.SelectedItem?.ToString();
            if (selectedDirection == null)
            {
                Console.WriteLine("Please select a transfer direction.");
                return;
            }
            bool isForwardTransfer = selectedDirection == "->";

            // Check if the boxBarcode exists in the database
            bool boxExists = await CheckBoxExistsAsync(boxBarcode);
            if (!boxExists)
            {
                // Create a new entry for the boxBarcode in the database
                await CreateNewBoxEntryAsync(boxBarcode);
                Console.WriteLine($"Created new entry for boxBarcode: {boxBarcode}");
            }

            // Check if the fixtureID exists in the database
            bool fixtureExists = await CheckFixtureExistsAsync(fixtureID);
            if (!fixtureExists)
            {
                // Create a new entry for the fixtureID in the database
                await CreateNewFixtureEntryAsync(fixtureID);
                Console.WriteLine($"Created new entry for fixtureID: {fixtureID}");
            }

            // Adjust start and end indices based on the direction of transfer
            int startBurIndex, endBurIndex;
            List<int> emptySlots = new List<int>();

            if (isForwardTransfer)
            {
                // Forward transfer: Box to Fixture
                startBurIndex = await GetNextAvailableBurIndexAsync(boxBarcode, true, true);
                endBurIndex = Math.Min(startBurIndex + 39, 100);
                emptySlots = await GetEmptySlotsAsync(boxBarcode, startBurIndex, endBurIndex, true, true);
            }
            else
            {
                // Reverse transfer: Fixture to Box
                startBurIndex = await GetNextAvailableBurIndexAsync(fixtureID, false, false);
                endBurIndex = Math.Min(startBurIndex + 39, 100);
                emptySlots = await GetEmptySlotsAsync(fixtureID, startBurIndex, endBurIndex, false, false);
            }

            // Log the complete list of empty slots found
            Console.WriteLine($"Empty slots: {string.Join(", ", emptySlots)}");

            if (emptySlots.Count > 0)
            {
                string transferDirectionMessage;

                string emptySlotsMessage;
                if (emptySlots.Count <= 5)
                {
                    emptySlotsMessage = string.Join(", ", emptySlots);
                }
                else
                {
                    emptySlotsMessage = string.Join(", ", emptySlots.Take(5)) + ", and more";
                }

                transferDirectionMessage = isForwardTransfer
                    ? $"Absent slots: {emptySlotsMessage}.\n\nTransfer burs from box {boxBarcode} to fixture {fixtureID}?"
                    : $"Absent slots: {emptySlotsMessage}.\n\nTransfer burs from fixture {fixtureID} to box {boxBarcode}?";

                // Show the message box with structured formatting
                transferDirectionMessage = transferDirectionMessage.Replace(". ", ".\n\n");

                var result = MessageBox.Show(transferDirectionMessage, "Confirm Transfer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    Console.WriteLine("Transfer cancelled by the user.");
                    return;
                }
            }

            var payload = new
            {
                fixtureID = fixtureID,
                boxBarcode = boxBarcode,
                startBurIndex = startBurIndex,
                endBurIndex = endBurIndex,
                direction = isForwardTransfer ? "forward" : "reverse",
                excludeSlots = emptySlots
            };

            var payloadJson = JsonConvert.SerializeObject(payload);

            try
            {
                Console.WriteLine("Invoking Lambda function...");
                Console.WriteLine($"Payload: {payloadJson}");

                var responsePayload = await Program.lambdaClient.InvokeLambdaFunctionAsync("unsdev-EngBurFixtureTransferLambda", payloadJson);

                Console.WriteLine($"Lambda function response payload: {responsePayload}");

                dynamic lambdaResponse = JsonConvert.DeserializeObject(responsePayload);
                int statusCode = lambdaResponse.statusCode;

                if (statusCode == 200)
                {
                    MessageBox.Show("Transfer completed!");
                    Console.WriteLine("Lambda function executed successfully.");

                    // Update the UI based on the direction of transfer
                    await UpdateSessionDataAsync(fixtureID, boxBarcode, startBurIndex, endBurIndex, isForwardTransfer ? "forward" : "reverse");

                    // Refresh the grids to reflect the current status
                    await PopulateBoxSlotsAsync(boxBarcode);
                    await PopulateFixtureSlotsAsync(fixtureID, isForwardTransfer, false);

                    // Display transfer completion message and reset UI
                    MessageBox.Show(isForwardTransfer
                        ? $"Burs have been transferred from box {boxBarcode} to the fixture {fixtureID}!"
                        : $"Burs have been transferred from fixture {fixtureID} into the box {boxBarcode}!");
                    Console.WriteLine(isForwardTransfer
                        ? $"Burs have been transferred from box {boxBarcode} to the fixture {fixtureID}."
                        : $"Burs have been transferred from fixture {fixtureID} into the box {boxBarcode}.");
                    ResetUI();
                }
                else
                {
                    string errorMessage = lambdaResponse.body ?? "Unknown error occurred.";
                    MessageBox.Show($"Transfer failed: {errorMessage}");
                    Console.WriteLine($"Lambda function execution failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking Lambda function: {ex.Message}");
                MessageBox.Show($"Error invoking Lambda function: {ex.Message}");
                Logger.LogError("Error invoking Lambda function", ex);
            }
        }

        private async Task<List<int>> GetEmptySlotsAsync(string id, int startBurIndex, int endBurIndex, bool isBox, bool isForwardTransfer)
        {
            var emptySlots = new List<int>();
            string tableName = isBox ? "EngBurBoxTable_TEST" : "EngBurQCTrayTable";
            var table = Table.LoadTable(AWSDynamo.dbClient, tableName);
            var filter = new QueryFilter(isBox ? "boxBarcode" : "trayId", QueryOperator.Equal, id);
            var search = table.Query(filter);

            try
            {
                var documentSet = await search.GetNextSetAsync();
                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];

                    // Log the document contents for debugging
                    Console.WriteLine($"Document retrieved: {document.ToJsonPretty()}");

                    for (int i = startBurIndex; i <= endBurIndex; i++)
                    {
                        string burKey = $"bur{i}";
                        string testBurKey = $"testbur{i}";

                        // Log each key being checked
                        Console.WriteLine($"Checking keys: {burKey} and {testBurKey}");

                        bool isBurEmpty = !document.ContainsKey(burKey) || IsEmptyEntry(document[burKey]);
                        bool isTestBurEmpty = !document.ContainsKey(testBurKey) || IsEmptyEntry(document[testBurKey]);

                        if (isBurEmpty && isTestBurEmpty)
                        {
                            emptySlots.Add(i);
                        }
                        else
                        {
                            // Log the value if the slot is considered not empty
                            if (!isBurEmpty) Console.WriteLine($"Slot {i} is not empty in {burKey}. Value: {document[burKey]}");
                            if (!isTestBurEmpty) Console.WriteLine($"Slot {i} is not empty in {testBurKey}. Value: {document[testBurKey]}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No documents found for the given ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching empty slots: {ex.Message}");
                Logger.LogError("Error fetching empty slots", ex);
            }

            return emptySlots;
        }

        private bool IsEmptyEntry(DynamoDBEntry entry)
        {
            if (entry is Document doc)
            {
                return !doc.Any();
            }
            return entry == null || entry.AsString() == string.Empty;
        }

        private async Task<(int startBurIndex, int endBurIndex)> GetFixtureIndicesAsync(string fixtureID)
        {
            var table = Table.LoadTable(AWSDynamo.dbClient, "EngBurQCTrayTable");
            var filter = new QueryFilter("trayId", QueryOperator.Equal, fixtureID);
            var search = table.Query(filter);

            try
            {
                var documentSet = await search.GetNextSetAsync();
                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];
                    int startBurIndex = int.Parse(document["startBurIndex"].ToString());
                    int endBurIndex = int.Parse(document["endBurIndex"].ToString());
                    return (startBurIndex, endBurIndex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fixture indices: {ex.Message}");
                Logger.LogError("Error fetching fixture indices", ex);
            }
            return (0, 0); // Default values if not found
        }




        // This method is used to update the session data for the specified box
        private async Task UpdateSessionDataAsync(string fixtureID, string boxBarcode, int startBurIndex, int endBurIndex, string direction)
        {
            try
            {
                // Generate a new sessionID
                string sessionID = Guid.NewGuid().ToString();

                // Create a new document with the session details
                var document = new Dictionary<string, AttributeValue>
        {
            { "sessionID", new AttributeValue { S = sessionID } },
            { "fixtureID", new AttributeValue { S = fixtureID } },
            { "boxBarcode", new AttributeValue { S = boxBarcode } },
            { "startBurIndex", new AttributeValue { N = startBurIndex.ToString() } },
            { "endBurIndex", new AttributeValue { N = endBurIndex.ToString() } },
            { "direction", new AttributeValue { S = direction } },
            { "lastUpdateTime", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
        };

                // Create the put item request
                var putItemRequest = new PutItemRequest
                {
                    TableName = "EngBurFixtureTransferSession",
                    Item = document
                };

                // Insert the new document into DynamoDB
                await AWSDynamo.dbClient.PutItemAsync(putItemRequest);

                Console.WriteLine($"Created new session with sessionID: {sessionID}, fixtureID: {fixtureID}, boxBarcode: {boxBarcode}, startBurIndex: {startBurIndex}, endBurIndex: {endBurIndex}, direction: {direction}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating session data: {ex.Message}");
                Logger.LogError("Error updating session data", ex);
            }
        }

        // This method is used to get the next available bur index for the specified trayID
        private async Task<int> GetNextAvailableBurIndexAsync(string id, bool isBox, bool isForwardTransfer)
        {
            string tableName = isBox ? "EngBurBoxTable_TEST" : "EngBurQCTrayTable";
            string keyName = isBox ? "boxBarcode" : "trayId";
            string burPrefix = (isBox || isForwardTransfer) ? "bur" : "testbur";

            var table = Table.LoadTable(AWSDynamo.dbClient, tableName);
            var filter = new QueryFilter(keyName, QueryOperator.Equal, id);
            var search = table.Query(filter);

            try
            {
                Console.WriteLine("Query initialized successfully.");

                var documentSet = await search.GetNextSetAsync();
                Console.WriteLine($"Fetched {documentSet.Count} documents.");

                if (documentSet.Count > 0)
                {
                    var document = documentSet[0];

                    // Iterate over the bur entries in the document
                    for (int i = 1; i <= 100; i++)
                    {
                        string burKey = $"{burPrefix}{i}";

                        if (document.TryGetValue(burKey, out DynamoDBEntry burEntry))
                        {
                            // Check if the burEntry is a Document and if it's not empty
                            if (burEntry is Document burData && burData != null && burData.Count > 0)
                            {
                                // Check the values of P1 and P2
                                string p1 = burData.TryGetValue("P1", out DynamoDBEntry p1Entry) ? p1Entry.AsString() : "0";
                                string p2 = burData.TryGetValue("P2", out DynamoDBEntry p2Entry) ? p2Entry.AsString() : "0";

                                if (p1 == "0" && p2 == "0")
                                {
                                    Console.WriteLine($"Next available bur index: {i}");
                                    return i; // Return the next available index
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Next available bur index: {i}");
                            return i; // Return the next available index if burKey is not found
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No documents found for the specified ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching next available bur index: {ex.Message}");
                Logger.LogError("Error fetching next available bur data", ex);
            }

            Console.WriteLine("No available bur index found, defaulting to 1.");
            return 1; // Default to 1 if no available index is found
        }


        // Helper method to check if all fixture slots are filled
        private bool IsFixtureFilled()
        {
            foreach (var button in fixtureButtons)
            {
                if (button.BackColor != Color.DeepSkyBlue)
                {
                    return false;
                }
            }
            return true;
        }

        // Helper method to update the UI slots after transfer based on direction
        private void UpdateUISlotsAfterTransfer(string primaryID, string secondaryID)
        {
            if (comboBox1.SelectedItem.ToString() == "->")
            {
                // Forward transfer: Update fixture slots
                for (int i = 0; i < 40; i++)
                {
                    fixtureButtons[i].BackColor = boxButtons[i].BackColor;
                    boxButtons[i].BackColor = Color.Silver; // Mark box slots as empty after transfer
                }
            }
            else
            {
                // Reverse transfer: Update box slots
                for (int i = 0; i < 40; i++)
                {
                    boxButtons[i].BackColor = fixtureButtons[i].BackColor;
                    fixtureButtons[i].BackColor = Color.Silver; // Mark fixture slots as empty after transfer
                }
            }
        }
    }

}
