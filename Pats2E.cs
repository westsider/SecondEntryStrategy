#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class Pats2E : Strategy
	{
		
		private bool Debug = false;
		private bool AlertOn = true;
		
		// Long Entries
		private int LastBar = 0;
		private int NewHigBarnum = 0;
		private double NewHighPrice = 0.0;
		private int BarsSinceHigh = 0;
		private bool SeekingFirstEntry = false;
		private bool FoundFirstEntry = false;
		private int FirstEntryBarnum = 0;
		private int BarsSinceFirstEntry = 0;
		private bool SecondEntrySetupBar = false;
		private bool FoundSecondEntry = false;
		private string LineName = "na";
		private int SecondEntryBarnum = 0;
		private long startTime = 0;
		private	long endTime = 0;
		
		// Long trades
		private double SecodEntryLongTarget = 0.00;
		private double SecodEntryLongStop = 0.0;
		private bool 	FailedSecondEntry = false;
		private bool 	In2EfailTrade = false;
		private int 	LongTradeCount = 0;
		private int 	LongTradeLossCount = 0;
		private double 	LongRisk = 0;
		private double	LowerValue = 0.0;
		private double	UpperValue = 0.0;
		
		// Short Entries
		private int 	BarsSinceLow = 0;
		private int 	NewLowBarnum = 0;
		private double	NewLowPrice = 0.0;
		private bool	FoundFirstEntryShort = false;
		private bool	SecondEntrySetupBarShort = false;
		private int		FirstEntryBarnumShort = 0;
		private bool	FoundSecondEntryShort = false;
		private bool 	SeekingFirstEntryShort = false;
		private int 	BarsSinceFirstEntryShort = 0;
		private int 	SecondEntryBarnumShort = 0;
		private int 	TicksToRecalc = 0;
		private double 	Padding = 0;
		
		// Short trades
		private double 	SecodEntryShortTarget = 0.00;
		private double 	SecodEntryShortStop = 0.0;
		private bool 	FailedSecondEntryShort = false;
		private bool 	In2EfailShortTrade = false;
		private int		ShortTradeCount = 0;
		private int 	ShortTradeLossCount = 0;
		
		// failed 2nd entry 
		private int		ShortFailedTradeCount = 0;
		private int 	ShortFailedEntryWins = 0;
		private double 	Failed2ndEntryShortTgt = 0.0; 
		private bool 	SeekingFailed2ESTarget = false;
		
		private int		LongFailedTradeCount = 0;
		private int 	LongFailedEntryWins = 0;
		private double 	Failed2ndEntryLongTgt = 0.0; 
		private bool 	SeekingFailed2ELTarget = false;
		
		// buttons
		private System.Windows.Controls.Button myBuyButton;
		private System.Windows.Controls.Button mySellButton;
		private System.Windows.Controls.Grid   myGrid;
		private bool BuyButtonIsOn = true;
		private bool SellButtonIsOn = true;
		
		// atm
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		
		#region Manage State
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Pats2E";
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				SecondEntrySound					= @"secondEntry.wav";
				FirstEntrySound					= @"firstEntry.wav";
				FailedEntrySound				= @"FailedEntrySound.wav";
				ShowStats						= true;
				ShowTargetsAndStops				= true;
				TargetInTicks					= 8;
				SwingMarkers					= false;
				SwingPadding					= 2;
				SwingLookBack					= 15;
				SoundAlertsOn					= true;
				ShowFailed2ndEntries			= true;
				StartTime						= DateTime.Parse("07:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime							= DateTime.Parse("15:15", System.Globalization.CultureInfo.InvariantCulture);
				TextColor						= Brushes.DimGray;
				PivotColor						= Brushes.WhiteSmoke;
				ShortTextColor					= Brushes.Red;
				ShortPivotColor					= Brushes.WhiteSmoke;
				ATMstrategyName					= @"16T";
				NoteFont									= new SimpleFont("Arial", 12);  
				StatsBkgColor								= Brushes.WhiteSmoke;
				StatsBkgOpacity								= 90; 
				
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow();
				startTime = long.Parse(StartTime.ToString("HHmmss"));
			 	endTime = long.Parse(EndTime.ToString("HHmmss"));
			} 
			// Once the NinjaScript object has reached State.Historical, our custom control can now be added to the chart
			else if (State == State.Historical)
			  {
				  Print("State.Historical");
			    // Because we're dealing with UI elements, we need to use the Dispatcher which created the object
			    // otherwise we will run into threading errors...
			    // e.g, "Error on calling 'OnStateChange' method: You are accessing an object which resides on another thread."
			    // Furthermore, we will do this operation Asynchronously to avoid conflicts with internal NT operations
			    ChartControl.Dispatcher.InvokeAsync((() =>
			    {
			        // Grid already exists
			        if (UserControlCollection.Contains(myGrid))
			          return;
			 
			        // Add a control grid which will host our custom buttons
			        myGrid = new System.Windows.Controls.Grid
			        {
			          Name = "MyCustomGrid",
			          // Align the control to the top right corner of the chart
			          HorizontalAlignment = HorizontalAlignment.Right,
			          VerticalAlignment = VerticalAlignment.Bottom,
			        };
			 
			        // Define the two columns in the grid, one for each button
			        System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
			        System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
			 
			        // Add the columns to the Grid
			        myGrid.ColumnDefinitions.Add(column1);
			        myGrid.ColumnDefinitions.Add(column2);
			 
			        // Define the custom Buy Button control object
			        myBuyButton = new System.Windows.Controls.Button
			        {
			          Name = "MyBuyButton",
			          Content = "LONG",
			          Foreground = Brushes.White,
			          Background = Brushes.LimeGreen
			        };
			 
			        // Define the custom Sell Button control object
			        mySellButton = new System.Windows.Controls.Button
			        {
			          Name = "MySellButton",
			          Content = "SHORT",
			          Foreground = Brushes.White,
			          Background = Brushes.Red
			        };
			 
			        // Subscribe to each buttons click event to execute the logic we defined in OnMyButtonClick()
			        myBuyButton.Click += OnMyButtonClick;
			        mySellButton.Click += OnMyButtonClick;
			 
			        // Define where the buttons should appear in the grid
			        System.Windows.Controls.Grid.SetColumn(myBuyButton, 0);
			        System.Windows.Controls.Grid.SetColumn(mySellButton, 1);
			 
			        // Add the buttons as children to the custom grid
			        myGrid.Children.Add(myBuyButton);
			        myGrid.Children.Add(mySellButton);
			 
			        // Finally, add the completed grid to the custom NinjaTrader UserControlCollection
			        UserControlCollection.Add(myGrid);
			 
			    }));
			  }
		}

		#endregion
		
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < SwingLookBack)
				return;
 
			LastBar = CurrentBar -1;  
			// get bar size for re draw
			int Bsize = BarsPeriod.Value;
			double pctSize = (double)Bsize * 0.95;
			TicksToRecalc = (int)pctSize; 
			Padding = (double)SwingPadding * TickSize;
			SetBollingetBands();
			if (ToTime(Time[0]) < startTime  || ToTime(Time[0]) > endTime) { return; }
			
			///******************************************************************************************
			///*********************************	Long	*********************************************
			///******************************************************************************************
			
			///**************	find new high or within 1 tick of high  ***************
			
			if (High[0] >= MAX(High, SwingLookBack)[1] - TickSize ) { 
				RemoveDrawObject("NewHigh"+LastBar); 
				if ( SwingMarkers ) { Draw.Diamond(this, "NewHigh"+CurrentBar, false, 0, High[0] + Padding, PivotColor); }
				NewHigBarnum = CurrentBar;
				NewHighPrice = High[0];
				FoundFirstEntry = false;
				SecondEntrySetupBar = false;
				FirstEntryBarnum = 0;
				FoundSecondEntry = false;
				SeekingFailed2ELTarget = false;
				Failed2ndEntryLongTgt = NewHighPrice + 100.0;
			}
			
			///**************	find first entry long  ***************
	
			BarsSinceHigh = CurrentBar - NewHigBarnum;
			if (BarsSinceHigh >= 2) {SeekingFirstEntry = true;}
			double DistanceToHigh = NewHighPrice - High[0];
			if (DistanceToHigh > 1.0 && High[0] > High[1]  && SeekingFirstEntry && !FoundFirstEntry ) {
				Draw.Text(this, "1stEntry"+CurrentBar, "1", 0, Low[0] - Padding * 2, TextColor);
				SeekingFirstEntry = false;
				FoundFirstEntry = true;
				FirstEntryBarnum = CurrentBar;
				FailedSecondEntry = false;
				
				if ( AlertOn ) {
					Alert("FirstEntry"+CurrentBar, Priority.High, "First Entry", 
					NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ FirstEntrySound,10, 
					Brushes.Black, Brushes.Yellow);  
				}
			}
			
			// end of bar if low is lower than prior print, remove the text, add a 1 below
			if( FirstEntryBarnum == CurrentBar && Bars.TickCount >= TicksToRecalc ) {
				RemoveDrawObject("1stEntry"+CurrentBar);
				Draw.Text(this, "1stEntry"+CurrentBar, "1", 0, Low[0] - Padding * 2, TextColor);
			}
			
			// must break low of 1st entry +  must be lower than high of 1st entry
			int FirstEntryBarsAgo = CurrentBar - FirstEntryBarnum;
			if ( FoundFirstEntry && Low[0] < Low[FirstEntryBarsAgo] 
					&& Close[0] < High[FirstEntryBarsAgo] ) {
				 	SecondEntrySetupBar = true;
			}
			
			///**************	find second entry long ***************
			 
			if (IsFirstTickOfBar && FirstEntryBarnum != 0) {
				BarsSinceFirstEntry = CurrentBar - FirstEntryBarnum;
				if ( Debug ) 
					{ Print( Time[0].ToShortDateString() + " \t" + Time[0].ToShortTimeString() 
					+ " \t" + "BarNum: " + CurrentBar 
					+ " \t" + "BarsSinceFirstEntry: " + BarsSinceFirstEntry );
					}
			}
			
			if (BarsSinceFirstEntry >=2 && SecondEntrySetupBar && FoundFirstEntry && !FoundSecondEntry) {
				if( DistanceToHigh > 2.0 ) {
					double EntryPrice = High[1] + TickSize;
					LineName = "SecondEntryLine";
					SecodEntryLongStop = MIN(Low, 3)[1] - TickSize;
					LongRisk = (EntryPrice - SecodEntryLongStop) / TickSize;
					
					
					DrawSecondEntryLine(EntryPrice, LineName, LongRisk);
					if (High[0] > High[1] ) {
						Draw.TriangleUp(this, "2EL"+CurrentBar, false, 0, Low[0] -Padding * 2, TextColor);
						FoundSecondEntry = true;
						SecondEntryBarnum = CurrentBar;
						SecodEntryLongTarget = High[1] + TickSize + ((double)TargetInTicks * TickSize);
						LongTradeCount  += 1;
						if ( BuyButtonIsOn ) {  EnterWithATM(Long: true); }
						
						if ( ShowTargetsAndStops ) {
							Draw.Text(this, "tgt" + CurrentBar, "-", 0, SecodEntryLongTarget, TextColor);
							Draw.Text(this, "tgtv" + CurrentBar, "+", 0, UpperValue, TextColor);
							Draw.Text(this, "stop" + CurrentBar, "-", 0, SecodEntryLongStop, ShortTextColor);
							// show risk
							//Draw.Text(this, "LongRisk" + CurrentBar, LongRisk.ToString("N0"), 0, SecodEntryLongStop - (TickSize * 6), TextColor);
						}
						NewHighPrice = 0.0;
						RemoveDrawObject(LineName+CurrentBar);
						RemoveDrawObject(LineName+"Txt"+CurrentBar); 
						RemoveEntryLines(LineName);
						if ( AlertOn ) {
							Alert("secondEntry"+CurrentBar, Priority.High, "Second Entry", 
							NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ SecondEntrySound,10, 
							Brushes.Black, Brushes.Yellow);  
						}
					}
				}
			}
			
			// end of bar if low is lower than prior print, remove the triangle, add a triangle below
			if( SecondEntryBarnum == CurrentBar && Bars.TickCount >= TicksToRecalc ) {
				// RemoveDrawObject("SecondEntryLine"+CurrentBar);
				RemoveDrawObject("2EL"+CurrentBar);
				Draw.TriangleUp(this, "2EL"+CurrentBar, false, 0, Low[0] - Padding * 2, TextColor);
				
				SecodEntryLongStop = Low[0] - TickSize;
				RemoveDrawObject("stop"+CurrentBar);
				Draw.Text(this, "stop" + CurrentBar, "-", 0, SecodEntryLongStop, Brushes.Red);
			}
			
			
			///******************************************************************************************
			///*********************************	Short	*********************************************
			///******************************************************************************************
			
			
			
			///**************	find new low or within 1 tick of low  ***************
			
			if (Low[0] <= MIN(Low, SwingLookBack)[1] + TickSize ) { 
				RemoveDrawObject("NewLow"+LastBar); 
				//Draw.Diamond(this, "NewLow"+CurrentBar, false, 0, Low[0], PivotColor);
				if ( SwingMarkers ) { Draw.Dot(this, "NewLow"+CurrentBar, false, 0, Low[0] - Padding, ShortPivotColor);}
				NewLowBarnum = CurrentBar;
				NewLowPrice = Low[0];
				FoundFirstEntryShort = false;
				SecondEntrySetupBarShort = false;
				FirstEntryBarnumShort = 0;
				FoundSecondEntryShort = false;
				FailedSecondEntryShort = false; 
				SeekingFailed2ESTarget = false;
				Failed2ndEntryShortTgt = NewLowPrice - 100.0;
			}
			
			///**************	find first entry short  ***************
	
			BarsSinceLow = CurrentBar - NewLowBarnum;
			if (BarsSinceLow >= 2) {SeekingFirstEntryShort = true;}
			double DistanceToLow = Low[0] - NewLowPrice;
			if (DistanceToLow > 1.0 && Low[0] < Low[1]  && SeekingFirstEntryShort && !FoundFirstEntryShort ) {
				Draw.Text(this, "1stEntryShort"+CurrentBar, "1", 0, High[0] + Padding * 2, ShortTextColor);
				SeekingFirstEntryShort = false;
				FoundFirstEntryShort = true;
				FirstEntryBarnumShort = CurrentBar;
				if ( AlertOn ) {
					Alert("FirstEntryShort"+CurrentBar, Priority.High, "First Entry Short", 
					NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ FirstEntrySound,10, 
					Brushes.Black, Brushes.Yellow);  
				}
			}
			
			// end of bar if high is higher than prior print, remove the text, add a 1 below
			if( FirstEntryBarnumShort == CurrentBar && Bars.TickCount >= TicksToRecalc ) {
				RemoveDrawObject("1stEntryShort"+CurrentBar);
				Draw.Text(this, "1stEntryShort"+CurrentBar, "1", 0, High[0] + Padding * 2, ShortTextColor);
			}
			
			// must break high of 1st entry +  must be higher than low of 1st entry
			int FirstEntryBarsAgoShort = CurrentBar - FirstEntryBarnumShort;
			if ( FoundFirstEntryShort && High[0] > High[FirstEntryBarsAgoShort] 
					&& Close[0] > Low[FirstEntryBarsAgoShort] ) {
				 	SecondEntrySetupBarShort = true;
			}
					
			
			///**************	find second entry short ***************
			 
			if (IsFirstTickOfBar && FirstEntryBarnumShort != 0) {
				BarsSinceFirstEntryShort = CurrentBar - FirstEntryBarnumShort;
				if ( Debug ) 
					{ Print( Time[0].ToShortDateString() + " \t" + Time[0].ToShortTimeString() 
					+ " \t" + "BarNum: " + CurrentBar 
					+ " \t" + "BarsSinceFirstEntryShort: " + BarsSinceFirstEntryShort );
					}
			}
			
			if (BarsSinceFirstEntryShort >=2 && SecondEntrySetupBarShort && FoundFirstEntryShort && !FoundSecondEntryShort) {
				if( DistanceToLow > 2.0 ) {
					double EntryPrice = Low[1] - TickSize;
					SecodEntryShortStop = MAX(High, 3)[1] + TickSize;  
					double ShortRisk = (SecodEntryShortStop - EntryPrice) / TickSize;
					LineName = "SecondEntryLineShort";
					DrawSecondEntryLine(EntryPrice, LineName, ShortRisk);
					if (Low[0] < Low[1] ) {
						Draw.TriangleDown(this, "2ES"+CurrentBar, false, 0, High[0] + Padding * 2, ShortTextColor);
						FoundSecondEntryShort = true;
						SecondEntryBarnumShort = CurrentBar;
						
						SecodEntryShortTarget = Low[1] - TickSize - ((double)TargetInTicks * TickSize);
						SecodEntryShortStop = High[0] + TickSize;
						ShortTradeCount  += 1;
						
						if ( SellButtonIsOn ) { EnterWithATM(Long: false); }
						
						if ( ShowTargetsAndStops ) {
							Draw.Text(this, "tgtS" + CurrentBar, "-", 0, SecodEntryShortTarget, TextColor);
							Draw.Text(this, "tgtSv" + CurrentBar, "+", 0, LowerValue, TextColor);
							Draw.Text(this, "stopS" + CurrentBar, "-", 0, SecodEntryShortStop, ShortTextColor);
							// show risk
							//Draw.Text(this, "ShortRisk" + CurrentBar, ShortRisk.ToString("N0"), 0, SecodEntryShortStop + (TickSize * 6), ShortTextColor);
							RemoveEntryLines(LineName);
						}
						
						NewLowPrice = 0.0;
						RemoveDrawObject(LineName+CurrentBar);
						RemoveDrawObject(LineName+"Txt"+CurrentBar); 
						if ( AlertOn ) {
							Alert("secondEntryShort"+CurrentBar, Priority.High, "Second Entry Short", 
							NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ SecondEntrySound,10, 
							Brushes.Black, Brushes.Yellow);  
						}
					}
				}
			}
			
			
			// end of bar if high is higher than prior print, remove the triangle, add a triangle below
			if( SecondEntryBarnumShort == CurrentBar && Bars.TickCount >= TicksToRecalc ) {
				RemoveDrawObject("2ES"+CurrentBar);
				Draw.TriangleDown(this, "2ES"+CurrentBar, false, 0, High[0] + Padding * 2, ShortTextColor); 
				
				SecodEntryShortStop = High[0] + TickSize;
				RemoveDrawObject("stopS"+CurrentBar);
				Draw.Text(this, "stopS" + CurrentBar, "-", 0, SecodEntryShortStop, ShortTextColor);
			}
			
			///******************************************************************************************
			///**************************	Failed 2nd Entry Short	*************************************
			///**************************	Results In Long Entry	*************************************
			///******************************************************************************************
			
			// first check for target hit
			if (FoundSecondEntryShort &&  Low[1] < SecodEntryShortTarget ) {
				FailedSecondEntryShort = true;
				return;
			}
			
			if ( FoundSecondEntryShort && ShowFailed2ndEntries && IsFirstTickOfBar && !FailedSecondEntryShort ) {
				int BarsSinceEntry = CurrentBar - SecondEntryBarnumShort;
				if ( BarsSinceEntry >= 0 && BarsSinceEntry <= 6 )  {
					
					// check for failed 2nd entry
					if ( High[0] >= SecodEntryShortStop  ) {
						FailedSecondEntryShort = true;
						ShortTradeLossCount += 1;
						Draw.Text(this, "FailedSecondEntryShort" + CurrentBar, "-----X", 0, SecodEntryShortStop, TextColor);
						//RemoveDrawObject("FailedSecondEntry"+LastBar);
						//Draw.Line(this, "FailedSecondEntry", 2, SecodEntryLongStop, -5, SecodEntryLongStop, Brushes.Red);
						ShortFailedTradeCount += 1; 
						// alert
						if ( AlertOn ) {
							Alert("FailedSecondEntryShort"+CurrentBar, Priority.High, " Failed Second Entry Short", 
							NinjaTrader.Core.Globals.InstallDir+@"\sounds\"+ FailedEntrySound,10, 
							Brushes.Black, Brushes.Yellow); 	
						}
						Failed2ndEntryShortTgt = SecodEntryShortStop + ((double)TargetInTicks * TickSize);
						Draw.Text(this, "Failed2ndEntryShortTgtline" + CurrentBar, "---^", 0, Failed2ndEntryShortTgt, TextColor);
						SeekingFailed2ESTarget = true;
					} else {
						FailedSecondEntryShort = false;
					}
				}
			}
			
			// check for target hit
			// if orig target hit - dont mark win
			if ( Low[0] < SecodEntryShortTarget ) {
				SeekingFailed2ESTarget = false;
			}
			// if 2e tgt hit
			if ( SeekingFailed2ESTarget && FailedSecondEntryShort && High[0] > Failed2ndEntryShortTgt  && Low[0] < Failed2ndEntryShortTgt  ) {
				ShortFailedEntryWins +=1;
				//Draw.Dot(this, "LongFailedEntryWins" + CurrentBar, false, 0,Failed2ndEntryShortTgt, Brushes.Blue); 
				SeekingFailed2ESTarget = false;
			}
			
			 ShowStatistics();
			
		}

		
		#region ATM Strategy
		
		private void EnterWithATM(bool Long) {
			if (State < State.Realtime)
  			return;
			
			
			
			Print("\n--------------------------------------------------------------------");
			
			// Submits an entry limit order at the current low price to initiate an ATM Strategy if both order id and strategy id are in a reset state
			// **** YOU MUST HAVE AN ATM STRATEGY TEMPLATE NAMED 'AtmStrategyTemplate' CREATED IN NINJATRADER (SUPERDOM FOR EXAMPLE) FOR THIS TO WORK ****
			if (orderId.Length == 0 && atmStrategyId.Length == 0)
			{
				isAtmStrategyCreated = false;  // reset atm strategy created check to false
				atmStrategyId = GetAtmStrategyUniqueId();
				orderId = GetAtmStrategyUniqueId();
				if ( Long ) {
					AtmStrategyCreate(OrderAction.Buy, OrderType.Limit, Close[0], 0, TimeInForce.Day, orderId, ATMstrategyName, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
							isAtmStrategyCreated = true;
						Print("LE");
					});
				} else {
					AtmStrategyCreate(OrderAction.Sell, OrderType.Limit, Close[0], 0, TimeInForce.Day, orderId, ATMstrategyName, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
							isAtmStrategyCreated = true;
						Print("SE");
					});
				}
			}
			
			// Check that atm strategy was created before checking other properties
			if (!isAtmStrategyCreated)
				return;
			
			// Check for a pending entry order
			if (orderId.Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderId);
				Print("\n\t\tWe already have ATM targets and stops\n");
				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
				if (status.GetLength(0) > 0)
				{
					Print("\n\t\tstatus.GetLength(0) > 0\n");
					// Print out some information about the order to the output window
					Print(Time[0].ToShortTimeString() + " The entry order average fill price is: " + status[0]
					+ " filled amount: " + status[1]
					+ " order state: " + status[2]);

					// If the order state is terminal, reset the order id value
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderId = string.Empty;
				}
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
				atmStrategyId = string.Empty;

			if (atmStrategyId.Length > 0)
			{
				Print("\n\t\tatmStrategyId.Length > 0\n");
				// You can change the stop price
//				if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
//					AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", atmStrategyId);

				// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
				// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
				Print(Time[0].ToShortTimeString() + " market position: " + GetAtmStrategyMarketPosition(atmStrategyId) 
					+ " quantity: " + GetAtmStrategyPositionQuantity(atmStrategyId) 
					+ " average price: " + GetAtmStrategyPositionAveragePrice(atmStrategyId)
					+ " Unrealized PnL: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
			}
			Print("---------------------------------------------------------\n");
		}
		
		#endregion
	
		
		#region Helpers
		
		private void SetBollingetBands() {
			if (IsFirstTickOfBar) {
				UpperValue = Bollinger(2, 14).Upper[0]; 
				LowerValue = Bollinger(2, 14).Lower[0];
			}
		}
		
		// add line of prior high + 1 tick when searching for 2nd entry
		// problem, marking live without lower low
		private void DrawSecondEntryLine(double EntryPrice, string LineName, double Risk) {
			if (IsFirstTickOfBar) {
				Brush lineColor	= TextColor;
				double RiskSpacer = -2.0;
				if ( LineName == "SecondEntryLineShort" ) {
					//change color to red	
					lineColor	= ShortTextColor;
					RiskSpacer = 2.0;
					//EnterShortStopMarket(EntryPrice, "");
					//EnterWithATM(Long: false);
				} else {
					//EnterLongLimit(Convert.ToInt32(DefaultQuantity), EntryPrice, "");
					//EnterWithATM(Long: true);
				}
				if ( Debug ) 
				{ 
					Print("Sec Entry Line " + Time[0].ToShortDateString() + " \t" + Time[0].ToShortTimeString() 
					+ " \t" + " Barnum: " + CurrentBar 
					+ " \t" + " Ticks: " + Bars.TickCount.ToString());
				}
				RemoveEntryLines(LineName);
				Draw.Line(this, LineName+CurrentBar, 1, EntryPrice, -2, EntryPrice, lineColor);
				string PriceText = EntryPrice.ToString("N2");
				PriceText += "\nR: -" + Risk.ToString("N0");
				Draw.Text(this, LineName+"Txt"+CurrentBar, PriceText, -5, EntryPrice, lineColor); 
			}
		}
		
		private void RemoveEntryLines(string LineName) {
			RemoveDrawObject(LineName+LastBar);  
			RemoveDrawObject(LineName+"Txt"+LastBar); 
		}
		
		
		private void ShowStatistics() { 
			if (ShowStats) {
				string AllStats = "S t a t i s t i c s\n";
				AllStats += LongTradeCount + " Long Trades";
				int LongWins = LongTradeCount - LongTradeLossCount;
				//AllStats += "\nLong Wins " + LongWins;
				double WinPctLong = ((double)LongWins / (double)LongTradeCount) * 100;
				AllStats += "\n" + WinPctLong.ToString("N0") + "% Win";
				AllStats += "\n"; 
				
				AllStats += "\n" + ShortTradeCount + " Short Trades";
				int ShortWins = ShortTradeCount - ShortTradeLossCount;
				//AllStats += "\nShort Wins " + ShortWins;
				double WinPctShort = ((double)ShortWins / (double)ShortTradeCount) * 100;
				AllStats += "\n" + WinPctShort.ToString("N0") + "% Win";
				AllStats += "\n"; 
				
				if ( ShowFailed2ndEntries ) {
					if ( LongFailedTradeCount > 0 ) {
						AllStats += "\n" + LongFailedTradeCount  + " Failed 2E Long Trades";
						double WinPctFail2Elong = ((double)LongFailedEntryWins / (double)LongFailedTradeCount) * 100;
						AllStats += "\n" + WinPctFail2Elong.ToString("N0") + "% Win";
					}
					if ( ShortFailedTradeCount > 0 ) {
						AllStats += "\n" + ShortFailedTradeCount + " Failed 2E Short Trades";
						//AllStats += "\nShort Failed Wins " + ShortFailedEntryWins;
						double WinPctFail2Eshort = ((double)ShortFailedEntryWins / (double)ShortFailedTradeCount) * 100;
						AllStats += "\n" + WinPctFail2Eshort.ToString("N0") + "% Win";
					}
					AllStats += "\n";
				}
				
				Draw.TextFixed(this, "AllStats", AllStats, StatsLocation, TextColor, 
					NoteFont, Brushes.Transparent, StatsBkgColor, StatsBkgOpacity); 
			}
		}
		
		#endregion
		
		#region Button Logic
		// Define a custom event method to handle our custom task when the button is clicked
		private void OnMyButtonClick(object sender, RoutedEventArgs rea)
		{
		  System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
		  if (button != null) {
			  
			  if ( button.Name == "MyBuyButton") {
				 BuyButtonToggle( b: button); 
			  } else {
				  if ( SellButtonIsOn ) {
					  mySellButton.Background = Brushes.DarkRed;
					  mySellButton.Foreground = Brushes.Black;
				  } else {
					  mySellButton.Background = Brushes.Red;
					  mySellButton.Foreground = Brushes.White;
				  }
					SellButtonIsOn = !SellButtonIsOn;
				  	Print(button.Name + " Clicked and SellButtonIsOn is " + SellButtonIsOn);
			  }
		  }
		}
		
		private void BuyButtonToggle(System.Windows.Controls.Button b) {
			if ( BuyButtonIsOn ) {
				  myBuyButton.Background = Brushes.DarkGreen;
				  myBuyButton.Foreground = Brushes.Black;
			  } else {
				  myBuyButton.Background = Brushes.LimeGreen;
				  myBuyButton.Foreground = Brushes.White;
			  }
				BuyButtonIsOn = !BuyButtonIsOn;
				Print(b.Name + " Clicked and BuyButtonIsOn is " + BuyButtonIsOn);
		}
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="SecondEntrySound", Order=1, GroupName="Parameters")]
		public string SecondEntrySound
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FirstEntrySound", Order=2, GroupName="Parameters")]
		public string FirstEntrySound
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FailedEntrySound", Order=3, GroupName="Parameters")]
		public string FailedEntrySound
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowStatistics", Order=4, GroupName="Parameters")]
		public bool ShowStats
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowTargetsAndStops", Order=5, GroupName="Parameters")]
		public bool ShowTargetsAndStops
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TargetInTicks", Order=6, GroupName="Parameters")]
		public int TargetInTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SwingMarkers", Order=7, GroupName="Parameters")]
		public bool SwingMarkers
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SwingPadding", Order=8, GroupName="Parameters")]
		public int SwingPadding
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SwingLookBack", Order=9, GroupName="Parameters")]
		public int SwingLookBack
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SoundAlertsOn", Order=10, GroupName="Parameters")]
		public bool SoundAlertsOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowFailed2ndEntries", Order=11, GroupName="Parameters")]
		public bool ShowFailed2ndEntries
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartTime", Order=12, GroupName="Parameters")]
		public DateTime StartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="EndTime", Order=13, GroupName="Parameters")]
		public DateTime EndTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ATMstrategyName", Order=14, GroupName="Parameters")]
		public string ATMstrategyName
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Font", Description="Font", Order=4, GroupName="Statistics")]
		public SimpleFont NoteFont
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Location", Description="Stats Location", Order=5, GroupName="Statistics")]
		public TextPosition StatsLocation
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Background Color", Order=6, GroupName="Statistics")]
		public Brush StatsBkgColor
		{ get; set; }

		[Browsable(false)]
		public string StatsBkgColorSerializable
		{
			get { return Serialize.BrushToString(StatsBkgColor); }
			set { StatsBkgColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Background Opacity", Order=7, GroupName="Statistics")]
		public int StatsBkgOpacity
		{ get; set; }
		
		// ----------------------   colors   ---------------------------------------
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Long Text Color", Order=1, GroupName="Colors")]
		public Brush TextColor
		{ get; set; }

		[Browsable(false)]
		public string TextColorSerializable
		{
			get { return Serialize.BrushToString(TextColor); }
			set { TextColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="High Pivot Color", Order=2, GroupName="Colors")]
		public Brush PivotColor
		{ get; set; }

		[Browsable(false)]
		public string PivotColorSerializable
		{
			get { return Serialize.BrushToString(PivotColor); }
			set { PivotColor = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Short Text Color", Order=3, GroupName="Colors")]
		public Brush ShortTextColor
		{ get; set; }

		[Browsable(false)]
		public string ShortTextColorSerializable
		{
			get { return Serialize.BrushToString(ShortTextColor); }
			set { ShortTextColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Low Pivot Color", Order=4, GroupName="Colors")]
		public Brush ShortPivotColor
		{ get; set; }

		[Browsable(false)]
		public string ShortPivotColorSerializable
		{
			get { return Serialize.BrushToString(ShortPivotColor); }
			set { ShortPivotColor = Serialize.StringToBrush(value); }
		}	
		
		#endregion

	}
}
