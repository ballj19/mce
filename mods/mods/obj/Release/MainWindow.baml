   M S B A M L   `   `   `  ���� ?  ;mods, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:modsmods      mods.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     $localclr-namespace:mods     -     � Title$#  Modification Hub by Jake Ball��$	��805��$
��1100��.��� $	��770��$	��Top=� � ColumnDefinitions �� $
��941* ��� $
��151* ��� $��0* �.+��� -    ��Name$ SearchHistory��$��23��$
��Left=�$��Center=�$��
113,39,0,0q�$	��Top=�$	��100���� -    ��Name$ SearchButton��$��Search��$
��Left=�$��
298,40,0,0q�$	��Top=�$��99��$��22���� -    ��Name$ TextBox1��$
��Left=�$��23��$��
113,39,0,0q� �� TextWrapping$
 Wrap=�$�� ��$	��Top=�$��82���� -    ��Name$ Label1��$��Job Number:��$
��Left=�$��	30,36,0,0q�$	��Top=�$��26��$��78���� -    ��Name$ FilesListBox��$
��Left=�$	��210��$��	19,93,0,0q�$	��Top=�$	��274�� � ContextMenu �� .@�m� -   $��Remove��m� -   $��	Clear All��m� -	   $��Reload File���� -
   $ OpenFile��$
��Open��$
��Left=�$��
298,93,0,0q�$	��Top=�$��99��$��22���� -   $ 
BrowseFile��$��Browse��$
��Left=�$��
298,67,0,0q�$	��Top=�$��99��$��22���� -   $ FileExtension��$
��Left=�$��
218,39,0,0q�$	��Top=�$��75��$��22��� -   	 �Name$	 SearchProgress��
 � Minimum$
 0Q� � Maximum$ 29Q�$
��Left=�$��20��$��	19,68,0,0q�$	��Top=�$	��274���� -   $ 
OpenFolder��$��Open Folder��$
��Left=�$��298,120,0,0q�$	��Top=�$��99���� -   $ LDrive��$��L Drive��$
��Left=�$��298,147,0,0q�$	��Top=�$��99��$��22���� -   $ AdvancedSearch��$��Advanced Search��$
��Left=�$��298,200,0,0q�$	��Top=�$��99���� -    ��Name$ AllJobNumbersCheckBox��$4��.Search Under All Job Numbers (Takes ~1 Minute)��$
��Left=�$��	30,19,0,0q�$	��Top=�$	��282���� -   $ ExportExcel��$��Export Excel��$
��Left=�$��298,174,0,0q�$	��Top=�$��99��$��22���� -   $ MotionDummyFolder��$��Dummy Folder��$��99��$��22��$	��Top=�$
��Left=�$��298,147,0,0q��� -   $ 	KDMFolder��$��
KdM Folder��$��99��$��22��$	��Top=�$
��Left=�$��298,173,0,0q��� -   $ KDMEmail��$��	KDM Email��$��99��$��22��$	��Top=�$
��Left=�$��298,200,0,0q��� -    ��Name$ JobInfo��$
��Left=�$
��Wrap=�$	��Top=�$	��398��$	��450��$��
18,310,0,0q��� -   $ DifferentJobNumber��$4��.Search for this file in a different directory?��$y�Hidden=�$
��Left=�$��
60,385,0,0q�$	��Top=�$	��252��$��37���� -    ��Name$ InfoTabControl��$��
413,18,0,0q�$��2��.@��� -   $��LandingsTab��$��Landings��.���� $
��Auto=�.���� -    ��Name$ LandingConfig�� �� Orientation$ Vertical=�.+��� $ 
Horizontal=�.+��� -   $ 	TogglePIs��$��PI��$
��Left=�$��	10,10,0,0q�$	��Top=�$��46��$��28��$y�Hidden=��� -   $ LandingNormalHeader��$��0,10,0,0q�$
��Bold"�$
��Left=�$��Center=�$��Normal��$y�Hidden=�$	��Top=�
���   �$��2q�$��96��$��30���� -   $ LandingAltHeader��$��	-2,10,0,0q�$
��Bold"�$
��Left=�$��Center=�$��	Alternate��$y�Hidden=�$	��Top=�
���   �$��2q�$��96��$��30���� -   $ LandingConfigWrapper��$ 
Horizontal=�.+��� -   $ LandingLevels��$��	10,-2,0,0q�$�� �� �� 
IsReadOnly . 
������ $	��Top=�
���   �$��0q�$
��Left=�$��Center=�$��48��$��18���� -    $ 
LandingPIs��$��
-48,-2,0,0q�$�� �� . 
������ $	��Top=�
���   �$��0q�$
��Left=�$��Center=�$��48��$��18��$y�Hidden=��� -!   $ LandingNormalConfig��$��	-2,-2,0,0q�$�� �� . 
������ $	��Top=�
���   �$��0q�$
��Left=�$��Center=�$��96��$��18���� -"   $ LandingAltConfig��$��	-2,-2,0,0q�$�� �� . 
������ $	��Top=�
���   �$��0q�$
��Left=�$��Center=�$��96��$��18���� -#   $��IOTab��$��Inputs/Outputs��.���� $
��Auto=�.���  �� $	��39* ��� $	��28* �.+��� -$   $ ToggleIOView��$��Toggle View��$
��Left=�$��	10,10,0,0q�$	��Top=�$��99��$��22���� -%   $ ViewVersionIO��$��
V0.0.0 I/O��$
��Left=�$��
127,10,0,0q�$	��Top=�$��99��$��22���� -&   $ IOInfoSP��$ Vertical=�$��0,37,0,0q�$��2���� -'   $ BoardSP��$ Vertical=�$��0,37,0,0q�$��2���� -(   $��	HeaderTab��$��Headers��.���� -)   $��HeaderSP��$ 
Horizontal=��� -*    ��Name$ JobSummaryTab��$��Summary��.���� $��Visible=�$��24,0,0,0q�.���� -+   $ 
JobSummary��$
��Wrap=�$	��640��$	��Top=�$
��Left=��� -,   $��
OptionsTab��$��Options (Beta)��.��� .+��� $��Visible=�$	��Top=�$	��325��$
��Left=�$	��700��.���� --   $ LobbyOptionsBlock��$	��Top=�$	��300��$
��Left=�$
��Wrap=��� -.   $ BottomOptionsBlock��
������ $	��300��$	��Top=�$
��Left=�$��	350,0,0,0q�$	��670���� -/   $��
TracModTab��$��Trac Mod��.��� .+��� -0   $ TracModFilter��$
��Left=�$��23��$��5,10,0,0q�$
 Wrap=�$�� ��$	��Top=�$	��150���� $��Filter��$��23��$	��Top=�$
��Left=�$��50��$��	160,7,0,0q�$��Center=��� -1   $ TracModRefresh��$��Refresh��$��23��$��76��$
��Left=�$	��Top=�$��
958,10,0,0q��� -2   $ TracModLabelSP��$ 
Horizontal=�$��0,40,0,0q��� $��0,65,0,0q�.���� -3   $ TracModContentSP��$ Vertical=��� -4   $��
TracJobTab��$��Trac Job��.���� -5   $ TracJobRefresh��$��Refresh��$��23��$��76��$
��Left=�$	��Top=�$��
158,10,0,0q��� -6   $��
UtilityTab��$��Utility��.��� .+��� -7   $ ModDocs��$��Mod Document��$
��Left=�$��
10,117,0,0q�$	��Top=�$��99��$��21���� -8   $ Emulink��$��EMULINK��$
��Left=�$��	10,90,0,0q�$	��Top=�$��99��$��22���� -9   $ Mp2link��$��MP2LINK��$
��Left=�$��	10,63,0,0q�$	��Top=�$��99��$��22���� -:   $ OpenSim��$��
Create Sim��$
��Left=�$��	10,36,0,0q�$	��Top=�$��99��$��22���� -;   $ PTHCProgRecordComboBox��$
��Left=�$��
158,36,0,0q�$	��Top=�$	��120���� -<   $ PTHCProgRecord��$��PTHC Prog Rec��$
��Left=�$��
283,36,0,0q�$	��Top=�$��99��$��22���� -=   $ 
PTHCModDoc��$��PTHC Mod Doc��$
��Left=�$��
283,63,0,0q�$	��Top=�$��99��$��22���� ->   $ PrintsFolder��$��
Job Prints��$
��Left=�$��
10,143,0,0q�$	��Top=�$��99��$��22���� -?   $ ModPrintsBox��$��10,170,287,408q��� -@   $ OpenModFolder��$��Open Folder��$
��Left=�$��387,170,0,0q�$	��Top=�$��99��$��22���� -A   $��AdminTab��$��Admin��.��� .+��� -B   $ 
ModUpgrade��$��Upgrade��$��90��$��22��$	��Top=�$
��Left=�$��	10,10,0,0q��� -C   $ ArchiveButton��$��Archive��$��90��$��22��$	��Top=�$
��Left=�$��	10,37,0,0q��� -D   $ PersonalFileButton��$��Create Personal��$��90��$��22��$	��Top=�$
��Left=�$��	10,64,0,0q��� -E   $ N_EPLNK��$��N_EPNLK��$��90��$��22��$	��Top=�$
��Left=�$��	10,91,0,0q��� -F   $ CustomModButton��$��
Custom Mod��$��90��$��22��$	��Top=�$
��Left=�$��
10,118,0,0q��� -G   $ UpdateDatabase��$��	Update DB��$��90��$��22��$	��Top=�$
��Left=�$��
10,145,0,0q��� -H   $ ProgramMotion��$��Program Motion��$	��104��$��22��$	��Top=�$
��Left=�$��
242,37,0,0q�