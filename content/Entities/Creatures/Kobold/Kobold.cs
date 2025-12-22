
namespace TC2.Base.Components
{
	public static partial class Kobold
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,


		}

		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public partial struct Data: IComponent
		{
			public Kobold.Flags flags;

			[Save.Ignore, Net.Ignore] public float next_talk;
			[Save.Ignore, Net.Ignore] public float next_scream;

			public Data()
			{

			}
		}


		public static Sound.Handle[] snd_cough =
		{
			"kobold.cough.00",
			"kobold.cough.01",
			"kobold.cough.02",
			"kobold.cough.03",
			"kobold.grunt.00",
		};

		public static Sound.Handle[] snd_scream =
		{
			"kobold.scream.00",
			"kobold.scream.01",
			"kobold.bleh.00"
		};

		public static Sound.Handle[] snd_bleh =
		{
			"kobold.bleh.00",
		};

		public static Sound.Handle[] snd_move =
		{
			"vo_okay",
			"vo_yes",
			"vo_run"
		};

		public static Sound.Handle[] snd_ok =
		{
			"vo_okay",
			"vo_yes"
		};

		public static Sound.Handle[] snd_pickup =
		{
			"vo_give",
			"vo_need",
			"vo_look",
			"vo_strong",
		};

		public static Sound.Handle[] snd_drop =
		{
			"vo_okay",
			"vo_rip",
			"vo_build",
		};

		public static Sound.Handle[] snd_attack =
		{
			"vo_attack",
			"vo_kill",
			"vo_destroy",
			"vo_die"
		};

		public static Sound.Handle[] snd_pissed =
		{
			"vo_grrr",
			"vo_huh",
			"vo_no",
			"vo_rip",
			"vo_what"
		};

		public static Sound.Handle[] snd_insults =
		{
			"vo_bad",
			"vo_jerk",
			"vo_idiot",
			"vo_die",
		};

#if SERVER
		internal static string[] w_starters =
		[
			"ayy",
			"ay",
			"yar",
			"ohoho",
			"hoho",
			"hoh'",
			"wat",
			"ey",
			"yo",
			"hey",
			"oi",
			"oy",
			"mate"
		];

		internal static string[] w_enders_misc =
		[
			" innit",
			" ha",
			" ??",
			"!!!",
			"!?",
			" ye",
			" truly",
			"'yknow",
			" indeed",
			" hm...",
			" ok",
			" oke",
			" yes?",
			" ya?",
			" ugh...",
			" huh",
			" mate",
			" ey",
			" eyy",
			" ay",
			" surely",
			" mayb",
			"'it",
			" happen!",
			" evrytim",
			" woe",
			" hear'ye",
			"'ye",
			"'mate",
			"'ait",
			"'aight",
			" aight",
			" alright?",
			" rite?",
			"'rite?",
			" not",
		];

		internal static string[] w_enders_upset =
		[
			"?",
			"??",
			"???",
			"!",
			"!!",
			"!!!",
			"!?",
			"!?!",
			"...!",
			"...?",
			"'fokin'!?",
			" aaaa",
			" happen!",
			" evrytim!!",
			" ay!!!?",
			"'ait!?",
			" no!?",
			"!!?",
			" innit!?",
			" argh",
			" fshhhhh!!",
			" huh??!",
			" ya?!",
			" wha??!",
		];

		internal static string[] w_enders_nevermind =
		[
			"...or not",
			"...nevemind",
			" or...",
			" wait no",
			"...wait",
			"...noooo!!",
			" or not",
			"..fug...",
			"..no",
			"...not",
			"..nah",
			"..fshhhh",
			"--",
			"...uh?"
		];

		internal static string[] w_enders_curious =
		[
			"?",
			".??",
			"...?",
			"..huh?",
			"..uh?",
			" hm?",
			" huh?",
			" huh",
			"'innit?",
			" yes?",
			" ya?",
			" rite?",
			"'rite?",
			"'yknow???",
			"...or??",
			" or...?",
			" hmm??",
			" hmm....",
		];

		internal static string[] w_enders_low =
		[
			" ugh",
			" hhh",
			"...",
			"..uf",
			" eh",
			" pfft...",
			" pff",
			"..",
			" eee...",
			"'fsgh...",
			" bleh..",
			" bleh",
			" huhu?",
			" huhul?",
			"--",
			" oof",
			" foof",
			"'yknow...",
			"..hm",
			" oh...",
			" hhhh...",
			" fss...",
			"'gd..",
			"'it...",
			" grr..",
			" uh....",
			"...or",
			" or...",
			"...nevermind",
			"...oh",
			"...duh",
			"...pff",
			"..maybe.",
		];

		internal static string[] w_verbs_bad =
		[
			"eat",
			"drink",
			"find",
			"cry",
			"curse",
			"go to",
			"go",
			"perish",
			"die",
			"screw",
			"fuck",
			"shit",
			"damn",
			"dam",
			"grab",
			"nib",
			"knob",
			"smell",
			"commit",
			"do",
			"fall",
			"jump",
		];

		internal static string[] w_verbs_misc =
		[
			"eat",
			"drink",
			"find",
			"cry",
			"curse",
			"go to",
			"go",
			"suck",
			"shake",
			"screw",
			"fuck",
			"work",
			"do",
			"steal",
			"take",
			"give",
			"jag",
			"crawl",
			"kick",
			"shit",
			"blow",
			"smell",
			"grab",
			"nib",
			"fall",
			"jump",
			"sit",
			"dance",
			"bounce",
			"slap",
			"kiss"
		];

		internal static string[] w_learn =
		[
			"learn",
			"hear",
			"know",
			"see",
			"saw",
			"find",
			"discover",
			"listen",
			"catch"
		];

		internal static string[] w_that =
		[
			"that",
			"dat",
			"those",
			"these",
			"the"
		];

		internal static string[] w_our =
		[
			"our",
			"us",
			"ours",
			"kobold",
			"kobs",
		];

		internal static string[] w_and =
		[
			"and",
			"plus",
			"also",
			"along",
			"and too",
			"withal"
		];

		internal static string[] w_your =
		[
			"ye",
			"yours",
			"yer",
			"ya",
			"yo",
			"yar",
			"thee",
		];

		internal static string[] w_you =
		[
			"ye",
			"yer",
			"ya",
			"yo",
			"thou",
			"u"
		];

		internal static string[] w_directions =
		[
			"off",
			"in",
			"thru",
			"onto",
			"off",
			"with",
			"up",
			"on",
			"under",
			"into",
			"outta",
			"out",
			"over"
		];

		internal static string[] w_targeters =
		[
			"ya",
			"ye",
			"yo",
			"your",
			"thee",
			"this",
			"that",
			"me",
			"mine"
		];

		internal static string[] w_question =
		[
			"how",
			"when",
			"wen",
			"why",
			"where",
			"who",
		];

		internal static string[] w_me =
		[
			"i",
			"am",
			"my",
			"kobs",
		];

		internal static string[] w_us =
		[
			"we",
			"kobold",
			"us",
			"our",
			"evyrone",
			"other",
			"frends",
			"kobs"
		];

		internal static string[] w_my =
		[
			"my",
			"me",
			"mine"
		];

		internal static string[] w_i_am =
		[
			"am",
			"i'm",
			"me",
			"i"
		];

		internal static string[] w_their =
		[
			"their",
			"them",
			"theirs",
			"those",
			"these",
			"the",
			"there"
		];

		internal static string[] w_they =
		[
			"they",
			"them",
			"dem",
			"all",
			"'em"
		];

		internal static string[] w_as =
		[
			"like",
			"lik",
			"as",
			"just",
			"alike"
		];

		internal static string[] w_is =
		[
			"is",
			"be",
			"has"
		];

		internal static string[] w_want =
		[
			"wanna",
			"want",
			"wan",
			"need",
			"crave",
			"wish",
			"demand",
		];

		internal static string[] w_have =
		[
			"has",
			"have",
			"own",
			"owns",
			"contains",
			"haves",
			"carry"
		];


		internal static string[] w_it =
		[
			"it",
			"some",
			"one",
			"them",
			"this",
			"such",
			"that",
		];

		internal static string[] w_but =
		[
			"but",
			"tho",
			"however",
			"altough",
			"except",
			"nobbut",
			"yet",
			"tough",
			"though",
			"thought",
			"natheless",
		];

		internal static string[] w_i_will =
		[
			"i'ma",
			"imma",
			"ima",
			"am gonna",
			"am gunna",
			"i shall",
			"i'll",
			"i will"
		];

		internal static string[] w_adjectives =
		[
			"'lil",
			"weenie",
			"poor",
			"thick",
			"soft",
			"hairy",
			"smelly",
			"bad",
			"green",
			"red",
			"yellow",
			"pink",
			"small",
			"long",
			"wide",
			"short",
			"no"
		];

		internal static string[] w_objects_vulgar =
		[
			"jerk",
			"piss",
			"crap",
			"fug",
			"shit",
			"shite",
			"gobshite",
			"fuck",
			"bastard",
			"shitter",
			"turd",
			"bollocks",
			"moran",
			"bellend",
			"bum",
			"pecker",
			"snot",
			"wank",
			"scum",
			"worm",
			"spunk",
			"gnat",
		];

		internal static string[] w_bad =
		[
			"dumb",
			"bad",
			"stupid",
			"lame",
			"shite",
			"shat",
			"small",
			"poor",
			"ugly",
			"broke",
			"broked",
			"bork",
			"rusty",
			"junked",
			"slow",
			"dented",
			"old",
			"failed",
			"fail",
			"smelly",
			"crappy",
			"crap",
		];

		internal static string[] w_adjectives_vulgar =
		[
			"dumb",
			"bad",
			"stupid",
			"lame",
			"pissed",
			"shite",
			"shat",
			"fucked",
			"fuck'd",
			"fucka",
			"cocked",
			"fat",
			"small",
			"big",
			"drunk",
			"flat",
			"domed",
			"junked",
			"fokin'",
			"fookin'"
		];

		internal static string[] w_cool =
		[
			"shiny",
			"good",
			"best",
			"big",
			"strong",
			"power",
			"large",
			"rich",
			"new",
			"fast",
			"red",
			"grand"
		];

		internal static string[] w_become =
		[
			"be",
			"become",
			"turn",
			"get",
			"grow",
			"develop",
			"gain",
			"emerge",
			"metamorsel"
		];

		internal static string[] w_defile =
		[
			"jerk",
			"piss",
			"shit",
			"crap",
			"fuck",
			"wank",
			"jack",
			"hump",
			"screw",
			"blow",
			"suck",
		];

		internal static string[] w_fear =
		[
			"cry",
			"run",
			"scream",
			"shake",
			"wail",
			"dread",
			"tremble",
			"fret",
		];

		internal static string[] w_hear =
		[
			"hear",
			"know",
			"heard",
			"listen",
			"sound",
			"ear",
			"eardrop",
			"eavesdrop",
			"voice"
		];

		internal static string[] w_say =
		[
			"say",
			"talk",
			"inform",
			"tell",
			"told",
			"yell",
			"voice",
			"speak",
			"spoke",
			"whisper",
			"advice",
			"warn",
			"alert",
		];

		internal static string[] w_infosource =
		[
			"poster",
			"friend",
			"paper",
			"tale",
			"story",
			"leaf",
			"sign",
			"legend",
			"epitaph",
			"etipaph",
			"scroll",
			"newsman",
			"news",
			"voice",
			"brother",
			"cousin",
			"granma",
			"uncle",
			"grampa",
			"foreman",
			"chief",
			"solder",
			"soldier",
			"guard",
			"sound",
			"text",
			"film",
			"speaker",
			"display",
			"radio",
			"someone",
			"kobs",
			"giant",
			"human",
			"they",
			"them",
		];

		internal static string[] w_take =
		[
			"grab",
			"take",
			"steal",
			"grip",
			"hand",
			"nick",
			"nab",
			"snatch",
			"confiscate",
			"capture",
			"seize",
		];

		internal static string[] w_destroy =
		[
			"burn",
			"destroy",
			"ruin",
			"crush",
			"explode",
			"detonate",
			"wreck",
			"blow up",
			"broke",
			"deface",
			"expunge",
			"sink",
			"stomp",
			"break",
			"trash",
			"crash",
			"damage",
			"raze",
			"shatter",
			"end",
			"waste"
		];

		internal static string[] w_fight =
		[
			"bash",
			"kick",
			"beat",
			"slam",
			"smash",
			"hit",
			"punch",
			"crush",
			"slash",
			"stab",
			"ax",
			"expunge",
			"tear",
			"rip",
			"stomp",
			"crack",
			"cut",
			"spit",
			"snap",
			"kill",
			"murder",
			"dump",
			"bounce",
			"destroy",
			"ruin",
			"shatter",
			"axe",
			"damage",
			"gut",
			"bang",
		];

		internal static string[] w_move =
		[
			"go",
			"run",
			"move",
			"walk",
			"scoot",
			"cruise",
			"skate",
			"dash",
			"slide",
			"crawl",
			"speed",
			"step",
			"skid",
			"tumble",
			"veer",
			"steer",
			"launch",
			"propel",
			"slither",
			"accelerate"
		];

		internal static string[] w_okay =
		[
			"alrite",
			"aight",
			"oke",
			"ok",
			"ye",
			"ait",
			"yeh",
			"yea",
			"yus",
			"gud",
			"yes",
			"hm",
			"yarp",
			"yee",
			"rite",
			"yuh"
		];

		internal static string[] w_disagree =
		[
			"nuh",
			"nop",
			"nope",
			"nah",
			"na",
			"piss off",
			"fuck off",
			"neva",
			"nay",
			"nye",
			"no",
			"not",
			"bad",
			"hnnn",
			"nuhuh",
			"eeee",
			"guh",
			"waht",
			"what",
			"wat",
			"whut",
			"wot",
			"woh",
			"rrr"
		];

		internal static string[] w_what =
		[
			"what",
			"wat",
			"wha",
			"wut",
			"whut",
		];

		internal static string[] w_insert =
		[
			"jam",
			"bash",
			"push",
			"shove",
			"stick",
			"stuff",
			"cram",
			"embed",
			"stick",
			"add"
		];

		internal static string[] w_work =
		[
			"work",
			"spin",
			"make",
			"do",
			"touch",
			"crank",
			"use",
			"turn",
			"power",
			"operate",
			"control",
			"produce",
			"skill",
			"commit",
		];

		internal static string[] w_eat =
		[
			"eat",
			"chomp",
			"devour",
			"bite",
			"gnaw",
			"consume",
			"chew",
			"swallow",
			"dinner",
			"snack",
			"feed"
		];

		internal static string[] w_for =
		[
			"for",
			"as",
			"fo'",
			"with",
			"wit"
		];

		internal static string[] w_into =
		[
			"in",
			"into",
			"to",
			"down",
			"forth",
			"up"
		];

		internal static string[] w_at =
		[
			"at",
			"onto",
			"on",
			"toward",
			"ontop",
			"after"
		];

		internal static string[] w_through =
		[
			"through",
			"thru",
			"forth",
			"to",
			"unto",
		];

		internal static string[] w_ed =
		[
			"'d",
			"ed",
			"d",
		];

		internal static string[] w_then =
		[
			"so",
			"then",
			"and",
			"also",
			"thus",
			"ergo",
			"cue",
			"next",
			"afters",
		];

		internal static string[] w_because =
		[
			"cus",
			"because",
			"bcuse",
			"cause",
			"since",
			"hence",
			"ergo",
			"for",
			"due",
			"as"
		];

		internal static string[] w_curse =
		[
			"fuck",
			"screw",
			"damn",
			"piss",
			"shit",
			"curse",
			"woe"
		];

		internal static string[] w_throw =
		[
			"throw",
			"toss",
			"fling",
			"hurl",
			"lob",
			"launch",
			"fly",
			"push",
			"catapult",
			"cast",
			"fall",
			"send",
			"flip",
			"sling",
			"swing",
			"cast"
		];

		internal static string[] w_watch =
		[
			"watch",
			"see",
			"keep",
			"protect",
			"protec",
			"defend",
			"safe",
			"save",
			"guard",
			"gourd",
			"help",
			"look",
			"survilance",
			"escort"
		];

		internal static string[] w_gather =
		[
			"mine",
			"dig",
			"excavate",
			"shovel",
			"chop",
			"find",
			"get",
			"pick",
			"accumulate",
			"capture",
			"confiscate",
			"procure",
			"accrue",
		];

		internal static string[] w_buy =
		[
			"buy",
			"get",
			"trade",
			"obtain",
			"acquire",
			"money",
			"take",
			"shop",
			"market",
			"transaction",
			"change"
		];

		internal static string[] w_discomfort =
		[
			"itch",
			"smell",
			"ache",
			"hurt",
			"burn",
			"suffer",
			"sore",
			"dull",
			"dry",
			"wet",
			"pains",
			"blunt",
			"bubble",
			"cold",
			"warm",
			"shake",
			"cracks",
			"stink",
			"numb",
			"tired",
		];

		internal static string[] w_time =
		[
			"today",
			"soon",
			"tomoro",
			"tody",
			"after",
			"now",
			"before",
			"then",
		];

		internal static string[] w_structures =
		[
			"house",
			"home",
			"tavern",
			"inn",
			"dorm",
			"school",
			"castle",
			"village",
			"town",
			"lamp",
			"city",
			"tower",
			"barn",
			"farm",
			"shack",
			"booth",
			"cabin",
			"lodge",
			"shop",
			"cottage",
			"residence",
			"abode",
			"adobe",
			"dormitory",
			"jester",
			"dwelling",
			"domicile",
			"castle",
			"tent",
			"stall",
			"brother",
			"subway",
			"factorio",
			"factorium",
			"chair",
			"gate",
			"skycraper",
			"foundry",
			"mill",
			"ditch",
			"kitchen"
		];

		internal static string[] w_authority =
		[
			"boss",
			"sire",
			"master",
			"liege",
			"lord",
			"big man",
			"love",
			"chief",
			"general",
			"leader",
			"ladder",
			"captain",
			"mayor",
			"mayo",
			"inspector",
			"foreman",
			"m'am",
			"dad",
			"mom",
			"forman",
			"empiror",
			"umpire",
			"man",
			"chef",
			"enginar",
			"enguith",
			"m'lord",
			"m'lady",
			"duke",
			"boron",
			"baron",
			"major",
			"majordomo",
		];

		internal static string[] w_supper =
		[
			"dinar",
			"dunner",
			"dinner",
			"supper",
			"feast",
			"eating",
			"meal",
			"breakfast",
			"lunch",
			"luncheon",
			"snack"
		];

		internal static string[] w_food =
		[
			"majonez",
			"pudding",
			"pudling",
			"puding",
			"schnitzel",
			"knodel",
			"bread",
			"chow",
			"oats",
			"crumb",
			"food",
			"sauce",
			"stew",
			"soup",
			"morsel",
			"cake",
			"pie",
			"beer",
			"pickles",
			"onion",
			"tomato",
			"potato",
			"carrot",
			"meat",
			"loaf",
			"meatloaf",
			"sausage",
			"susage",
			"liver",
			"porridge",
			"biscuit",
			"pretzel",
			"bagel",
			"samwich",
			"sandwich",
			"dumpling",
			"dumbling",
			"egg",
			"biscuit",
			"batter",
			"flour",
			"fish",
			"rice",
			"corn",
			"cheese",
			"mustard",
			"vinegar",
			"oil",
			"luncheon",
			"ham",
			"lard",
			"butter",
		];

		internal static string[] w_enemy =
		[
			"enemy",
			"demon",
			"bad",
			"monster",
			"fiend",
			"dumb",
			"moran",
			"agent",
			"spy",
			"rebel",
			"rebil",
			"disent",
			"bandit",
			"detractor",
			"traitor",
			"deserter",
			"desserter",
			"defector",
			"difector",
			"disenter",
			"impostor",
			"hobo",
			"hobold",
			"sabotaur",
			"sabotaurus",
			"saboter",
			"protestor",
			"sketpic",
			"dubass",
			"collaborateur",
			"colabolatur",
			"coraburator"
		];

		internal static string[] w_animal =
		[
			"mouse",
			"cat",
			"dog",
			"badger",
			"bagel",
			"twuk",
			"poh",
			"snake",
			"bird",
			"goat",
			"sheep",
			"owl",
			"dodo",
			"hedhedog",
			"hedgehog",
			"wolf",
			"hog",
			"crane",
			"rat",
			"mamoth",
			"wizard",
			"clown",
			"jester",
			"fish",
			"chicken",
			"duck",
			"cow",
			"horse",
			"pig",
			"peasant",
			"pheasant",
			"passant",
			"molusk",
			"snail",
			"dwarfe",
			"baby"
		];

		internal static string[] w_animal_cute =
		[
			"kitten",
			"kitty",
			"puppy",
			"mouse",
			"birdy",
			"doggy",
			"ferret",
			"tetrad",
			"chiken",
			"snail"
		];

		internal static string[] w_animal_big =
		[
			"cow",
			"poh",
			"bison",
			"mamoth",
			"wale",
			"whale",
			"wahale",
			"bigfish",
			"giant"
		];

		internal static string[] w_objects_misc =
		[
			"brick",
			"arse",
			"cart",
			"bucket",
			"belt",
			"jar",
			"head",
			"nose",
			"box",
			"flute",
			"puffin",
			"toe",
			"rag",
			"rug",
			"trumpet",
			"clock",
			"hole",
			"arse",
			"baba",
			"bag",
			"pile",
			"swamp",
			"heap",
			"monk",
			"priest",
			"hill",
			"house",
			"booth",
			"puddle",
			"bog",
			"boot",
			"hat",
			"pit",
			"haybale",
			"window",
			"spike",
			"boat",
			"barrow",
			"wood",
			"majonez",
			"goat",
			"sheep",
			"doorhandle",
			"plate",
			"meat",
			"knodel",
			"bread",
			"water",
			"can",
			"trash",
			"door",
			"handle",
			"knob",
			"donkey",
			"mayor",
			"sheriff",
			"king",
			"sack",
			"ball",
			"bell",
			"cow",
			"cat",
			"dog",
			"grass",
			"mum",
			"you",
			"pen",
			"crab",
			"basket",
			"card",
			"death",
			"dead",
			"corpse",
			"twuk",
			"poh",
			"cowo",
			"pus",
			"junk",
			"bar",
			"beard",
			"board",
			"scrap",
			"hose",
			"pole",
			"fence",
			"chalk",
			"eye",
			"rope",
			"pick",
			"axe",
			"money",
			"coin",
			"queen",
			"roof",
			"pants",
			"socks",
			"sock",
			"gum",
			"bum",
			"food",
			"chow",
			"live",
			"tractor",
			"life",
			"sis",
			"cunce",
			"bramble",
			"weed",
			"hair",
			"snail"
		];

		internal static string[] w_suffix_vulgar =
		[
			"brick",
			"arse",
			"cart",
			"bucket",
			"head",
			"nose",
			"box",
			"hole",
			"arse",
			"bag",
			"gunk",
			"pile",
			"jam",
			"ball",
			"sauce",
			"king",
			"jank",
			"bang",
			"bunk",
			"shack",
			"queen",
			"driver",
			"wipe",
			"house",
			"crack",
			"crank",
			"wreck",
			"broth",
			"stew",
			"mobile",
			"fuck",
			"flute",
			"muffin",
			"puffin",
			"turd",
			"tard",
			"lord",
			"keeper",
			"wad",
			"ward",
			"wanker",
			"bagel",
			"sock",
			"trumpet",
			"beard",
			"booth",
			"puddle",
			"toast",
			"spike",
			"spoon",
			"boat",
			"munk",
			"monk",
			"barrow",
			"smith",
			"wood",
			"grass"
		];

		internal static string[] w_weapons =
		[
			"gun",
			"pistal",
			"pistol",
			"shooter",
			"rifle",
			"pipe",
			"club",
			"bomb",
			"cowbar",
			"crowbar",
			"staff",
			"knife",
			"hammer",
			"sledge",
			"axe",
			"stick",
			"rebar",
			"mace",
			"bottle"
		];

		internal static string[] w_devices =
		[
			"device",
			"tool",
			"toy",
			"apparatus",
			"thing",
			"stuff",
			"utensil",
			"ware",
			"utility",
			"implement",
			"electron"
		];

		internal static string[] w_family =
		[
			"mum",
			"dad",
			"dog",
			"cat",
			"sis",
			"bruh",
			"nun",
			"grampa",
			"gramp",
			"gran",
			"granma",
			"granny",
			"auntie",
			"uncle",
			"babby",
			"boy",
			"gurl",
			"kobs",
			"missus",
			"fren",
			"jamarcus",
			"dreams"
		];

		internal static string[] w_bodypart =
		[
			"head",
			"nose",
			"eyes",
			"eye",
			"teeth",
			"toof",
			"arse",
			"knee",
			"chin",
			"skull",
			"feet",
			"skin",
			"hands",
			"innars",
			"innards",
			"appendix",
			"cranium",
			"toe",
			"finger",
			"hair",
			"beard",
			"gut",
			"neck",
			"legs",
			"pelvis",
			"spine",
			"fur"
		];

		internal static string[] w_comrades =
		[
			"comrades",
			"fren",
			"family",
			"squad",
			"crew",
			"kobs",
			"guys",
			"cowworkers",
			"workers",
			"company",
			"buysness",
			"shop",
			"ogranizaton",
			"group",
			"circle",
			"class",
			"bunch",
			"house",
			"fam",
			"kind",
			"stronks",
		];

		internal static string[] w_valuables =
		[
			"money",
			"gold",
			"wallet",
			"car",
			"coin",
			"key",
			"majonez",
			"food",
			"box",
			"jewel",
			"tractor",
			"traktor",
			"happy",
			"ring",
			"jewel",
			"hat",
			"nexus",
			"energy",
			"mason",
			"tank",
			"diamond",
			"dimond",
			"minerals",
			"biggest",
			"maximum",
			"power",
			"wealth",
			"sock",
			"iron",
			"feast",
			"convex",
			"foobal",
			"room",
			"mom",
			"mum",
			"snail",
			"dream",
			"spice",
			"fuel"
		];

		internal static string[] w_diseases =
		[
			"rickets",
			"fleas",
			"crickets",
			"pneummia",
			"coronavirus",
			"flu",
			"flue",
			"infulenza",
			"dementia",
			"dumb",
			"malus",
			"disease",
			"ill",
			"dead",
			"malapriapism",
			"dysfunction",
			"syndrome",
			"infecton",
			"deficency",
			"bacilus",
			"fungi",
			"prions",
			"virus",
			"bactery",
			"retrovirus",
			"aids",
			"schizofren",
			"schism",
			"lumbago",
			"herpes",
			"poverty",
			"craps",
			"tubecoloris",
			"stork",
			"pathology",
			"prognosis",
			"parasite",
			"worms"
		];

		public static string GetObjectName(ref XorRandom random, Material.Type material_type, Physics.Layer layer)
		{
			var sb = new System.Text.StringBuilder(32);

			switch (material_type)
			{
				case Material.Type.Wood:
				{
					sb.Append("wood");
				}
				break;

				case Material.Type.Stone:
				{
					sb.Append("stone");
				}
				break;

				case Material.Type.Metal:
				{
					sb.Append("metal");
				}
				break;

				case Material.Type.Mushroom:
				{
					sb.Append("shroom");
				}
				break;

				case Material.Type.Flesh:
				{
					sb.Append("meat");
				}
				break;

				case Material.Type.Powder:
				{
					sb.Append("dust");
				}
				break;

				case Material.Type.Foliage:
				{
					sb.Append("leaf");
				}
				break;

				case Material.Type.Fabric:
				{
					sb.Append("cloth");
				}
				break;

				default:
				{
					sb.Append("some");
				}
				break;
			}

			if (layer.HasAny(Physics.Layer.Door))
			{
				sb.Append(' ');
				sb.Append("door");
			}
			else if (layer.HasAny(Physics.Layer.Shipment | Physics.Layer.Crate | Physics.Layer.Storage))
			{
				sb.Append(' ');
				sb.Append("box");
			}
			else if (layer.HasAny(Physics.Layer.Building))
			{
				sb.Append(' ');
				sb.Append(w_structures.GetRandom(ref random));
			}
			else if (layer.HasAny(Physics.Layer.Item))
			{
				sb.Append(' ');
				sb.Append(w_devices.GetRandom(ref random));
			}
			else if (layer.HasAny(Physics.Layer.Resource))
			{

			}
			else
			{
				sb.Append(' ');
				sb.Append("stuff");
			}

			return sb.ToString();
		}

		//[ISystem.Event<Commandable.OrderEvent>(ISystem.Mode.Single, ISystem.Scope.Region)]
		//[Exclude<Player.Data>(Source.Modifier.Shared), HasTag("dead", false, Source.Modifier.Owned)]
		//public static void OnOrderEvent(ISystem.Info info, Entity entity, ref XorRandom random, ref Commandable.OrderEvent data,
		//[Source.Owned] ref Commandable.Data commandable,
		//[Source.Owned, Original] ref AI.Behavior behavior, [Source.Owned, Override] ref AI.Behavior behavior_override,
		//[Source.Owned, Original] ref AI.Movement movement, [Source.Owned, Override] ref AI.Movement movement_override,
		//[Source.Owned, Original] ref AI.Data ai, [Source.Owned] ref Pathfinding.State pathfinding_state,
		//[Source.Owned] ref Transform.Data transform, [Source.Owned, Optional] in Faction.Data faction)
		//{

		//}

		[Shitcode]
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.10f)]
		[HasComponent<Player.Data>(Source.Modifier.Parent, false), HasTag("dead", false, Source.Modifier.Owned), HasTag("kobold", true, Source.Modifier.Parent)]
		public static void HeadUpdate(ISystem.Info info, ref XorRandom random, Entity entity, Entity ent_speech_bubble, ref Region.Data region,
		[Source.Parent, Original] ref AI.Data ai_original, [Source.Parent, Override] ref AI.Data ai_override, [Source.Parent, Original] ref AI.Behavior behavior_original,
		[Source.Parent, Override] in Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Parent] ref Kobold.Data kobold, [Source.Parent] ref Commandable.Data commandable,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Head.Data head, [Source.Owned] ref Head.State head_state, 
		[Source.Parent, Optional(true)] ref SpeechBubble.Data speech_bubble, [Source.Owned] ref Body.Data body)
		{
			var time = info.WorldTime;

			var skip_timer = false; // (time - commandable.last_time) < 0.15f;

			//if (ai_original.anger > 800.00f)
			//{
			//	ai_override.stance = AI.Stance.Aggressive;
			//}

			//App.WriteLine($"{ai_original.stance} {ai_original.anger}");

			if (organic_state.unconscious_time > 0.50f)
			{
				head_state.t_next_sound = Maths.Min(head_state.t_next_sound, time + 2.50f);
			}

			var pain_delta = Maths.Max(organic_state.pain, 0.00f);
			//App.WriteLine($"{pain_delta}");

			//if (time >= kobold.next_scream && ((organic_state.consciousness_shared > 0.80f && pain_delta >= 1000.00f) || ((time - commandable.last_time) < 0.50f && commandable.last_type == Commandable.OrderType.Attack) || (ai_override.anger >= 750.00f)))
			//if (time >= kobold.next_scream && ((organic_state.consciousness_shared > 0.80f && pain_delta >= 1000.00f) || ((behavior_original.type == AI.Behavior.Type.Flee) || (ai_override.anger >= 750.00f))))
			if (time >= kobold.next_scream && ((organic_state.consciousness_shared > 0.80f && pain_delta >= 1000.00f) || ((ai_override.anger >= 750.00f))))
			{
				if (random.NextBool(0.90f))
				{
					Sound.Play(ref region, snd_scream.GetRandom(ref random), transform.position, volume: random.NextFloatRange(0.80f, 1.00f), pitch: random.NextFloatRange(0.98f, 1.02f) * head.voice_pitch, dist_multiplier: 1.20f);
					head_state.t_next_sound = time + random.NextFloatRange(1.50f, 3.00f);
				}
				kobold.next_scream = time + random.NextFloatRange(10.00f, 20.00f);
			}

			if (time >= head_state.t_next_sound || skip_timer)
			{
				//App.WriteLine(organic_state.efficiency);
				if (organic_state.consciousness_shared > 0.10f && (organic_state.unconscious_time > 3.00f || (organic_state.efficiency < 0.50f && organic_state.pain > 50.00f)))
				{
					var lerp = Maths.Normalize01Fast(organic_state.unconscious_time, 10.00f);

					Sound.Play(ref region, snd_cough.GetRandom(ref random), transform.position, volume: 0.35f * Maths.Lerp(1.00f, 0.50f, lerp), pitch: random.NextFloatRange(0.90f, 1.10f) * Maths.Lerp(1.00f, 0.80f, lerp) * head.voice_pitch);
					head_state.t_next_sound = time + random.NextFloatRange(1.50f, 2.50f + lerp);

					body.AddImpulse(random.NextUnitVector2Range(50.00f, 150.00f));

					if (time >= kobold.next_talk)
					{
						if (speech_bubble.IsNotNull())
						{
							var sb = new System.Text.StringBuilder(128);

							var index = random.NextIntRange(0, 3);
							//App.WriteLine(index);

							switch (index)
							{
								case 0:
								{
									sb.Append("not again");
								}
								break;

								case 1:
								{
									sb.Append(w_curse.GetRandom(ref random));
									sb.Append(' ');
									sb.Append(w_you.GetRandom(ref random));
								}
								break;

								case 2:
								{
									if (random.NextBool(0.50f))
									{
										sb.Append(w_you.GetRandom(ref random));
									}
									else
									{
										sb.Append(w_your.GetRandom(ref random));
										sb.Append(' ');

										if (random.NextBool(0.30f))
										{
											sb.Append(w_adjectives_vulgar.GetRandom(ref random));
											sb.Append(' ');
										}

										if (random.NextBool(0.90f))
										{
											sb.Append(w_family.GetRandom(ref random));
										}
										else
										{
											sb.Append(w_objects_misc.GetRandom(ref random));
										}
									}

									sb.Append(" will pay for this");
								}
								break;

								case 3:
								{
									sb.Append(w_curse.GetRandom(ref random));
									sb.Append(' ');
									sb.Append("this");
								}
								break;

								default:
								{
									if (random.NextBool(0.50f))
									{
										sb.Append(w_adjectives_vulgar.GetRandom(ref random));
										sb.Append(' ');
									}

									sb.Append(w_objects_vulgar.GetRandom(ref random));

									if (random.NextBool(0.50f))
									{
										sb.Append(w_objects_misc.GetRandom(ref random));
									}
								}
								break;
							}

							if (organic_state.pain > 500.00f || organic_state.unconscious_time > 1.00f)
							{
								sb.Length -= (int)(sb.Length * random.NextFloatRange(0.00f, lerp) * 0.90f);

								sb.Append("...");
							}

							var text = sb.ToString().Trim();

							speech_bubble.text = text;
							speech_bubble.Sync(ent_speech_bubble, true);

							kobold.next_talk = time + random.NextFloatRange(2.00f, 3.00f);
							head_state.t_next_sound = time + 1.50f;
						}
					}
				}
				else
				{
					if (organic_state.efficiency < 0.60f)
					{
						if (organic_state.pain > 2000.00f)
						{

						}
					}
					else
					{
						if (time >= kobold.next_talk || skip_timer)
						{
							if (speech_bubble.IsNotNull())
							{
								var sb = new System.Text.StringBuilder(128);

								if (ai_original.anger >= 200.00f)
								{
									var index = random.NextIntRange(0, 16);
									//App.WriteLine(index);

									switch (index)
									{
										case 0:
										{
											if (random.NextBool(0.40f))
											{
												sb.Append($"{w_starters.GetRandom(ref random)} ");
												if (random.NextBool(0.30f)) sb.Append($"{w_enders_upset.GetRandom(ref random)} ");
											}

											sb.Append($"{w_verbs_misc.GetRandom(ref random)} ");
											if (random.NextBool(0.60f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
											if (random.NextBool(0.40f)) sb.Append($"{w_adjectives.GetRandom(ref random)} ");

											if (random.NextBool(0.40f))
											{
												sb.Append(w_objects_vulgar.GetRandom(ref random));
												if (random.NextBool(0.50f))
												{
													sb.Append(w_suffix_vulgar.GetRandom(ref random));
												}
											}
											else
											{
												sb.Append(w_objects_misc.GetRandom(ref random));
											}

											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 1:
										{
											sb.Append($"{w_you.GetRandom(ref random)} ");
											if (random.NextBool(0.70f)) sb.Append($"{w_adjectives.GetRandom(ref random)} ");
											sb.Append(w_objects_vulgar.GetRandom(ref random));
											if (random.NextBool(0.80f)) sb.Append(w_suffix_vulgar.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 2:
										{
											sb.Append($"{w_you.GetRandom(ref random)} ");
											sb.Append($"{w_verbs_misc.GetRandom(ref random)} ");
											sb.Append($"{w_you.GetRandom(ref random)} ");
											if (random.NextBool(0.80f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
											if (random.NextBool(0.50f)) sb.Append($"{w_me.GetRandom(ref random)} ");
											sb.Append(w_objects_misc.GetRandom(ref random));
											if (random.NextBool(0.50f)) sb.Append(w_suffix_vulgar.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 3:
										{
											sb.Append($"{w_your.GetRandom(ref random)} ");

											if (random.NextBool(0.60f))
											{
												sb.Append(w_family.GetRandom(ref random));
											}
											else
											{
												sb.Append(w_objects_misc.GetRandom(ref random));
											}

											if (random.NextBool(0.50f)) sb.Append($" is");
											sb.Append($" {w_verbs_misc.GetRandom(ref random)} ");
											sb.Append(w_objects_misc.GetRandom(ref random));
											if (random.NextBool(0.50f)) sb.Append(w_suffix_vulgar.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 4:
										{
											if (random.NextBool(0.70f))
											{
												sb.Append(w_verbs_bad.GetRandom(ref random));
												if (random.NextBool(0.80f))
												{
													if (random.NextBool(0.50f))
													{
														sb.Append($" {w_directions.GetRandom(ref random)}");
													}

													sb.Append($" {w_targeters.GetRandom(ref random)}");
												}

												sb.Append(' ');
											}

											sb.Append(w_objects_vulgar.GetRandom(ref random));
											//if (random.NextBool(0.50f)) sb.Append(w_objects.GetRandom(ref random));						

											if (random.NextBool(0.50f)) sb.Append(w_suffix_vulgar.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 5:
										{
											sb.Append($"{w_you.GetRandom(ref random)} ");
											sb.Append($"{w_verbs_bad.GetRandom(ref random)}ed ");

											if (random.NextBool(0.40f))
											{
												if (random.NextBool(0.50f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
												if (random.NextBool(0.50f)) sb.Append($"{w_your.GetRandom(ref random)} ");
												sb.Append(w_objects_misc.GetRandom(ref random));
											}

											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 6:
										{
											if (random.NextBool(0.40f))
											{
												sb.Append(w_you.GetRandom(ref random));
											}
											else
											{
												sb.Append($"{w_your.GetRandom(ref random)} ");
												sb.Append(w_family.GetRandom(ref random));
											}

											sb.Append($" eat ");
											sb.Append($"{w_objects_misc.GetRandom(ref random)}");
											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 7:
										{
											sb.Append($"{w_me.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_defile.GetRandom(ref random)}");
											sb.Append(' ');
											if (random.NextBool(0.50f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
											sb.Append($"{w_your.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_family.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 8:
										{
											sb.Append($"{w_you.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_defile.GetRandom(ref random)}");
											sb.Append(' ');
											if (random.NextBool(0.50f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
											sb.Append($"{w_your.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_family.GetRandom(ref random)}");
											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 9:
										{
											sb.Append(w_your.GetRandom(ref random));
											sb.Append(' ');
											sb.Append($"{w_family.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_defile.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_you.GetRandom(ref random)}");
											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 10:
										{
											sb.Append($"{w_your.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_family.GetRandom(ref random)}");
											sb.Append($" is ");
											if (random.NextBool(0.40f)) sb.Append($"{w_directions.GetRandom(ref random)} ");
											sb.Append($"{w_your.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_family.GetRandom(ref random)}");
											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 11:
										{
											//sb.Append($"{w_me.GetRandom(ref random)}");
											sb.Append('i');
											sb.Append($" sware on me ");
											sb.Append($"{w_family.GetRandom(ref random)}");
										}
										break;

										case 12:
										{
											sb.Append($"{w_your.GetRandom(ref random)} ");
											sb.Append($"{w_family.GetRandom(ref random)} is so ");
											sb.Append($"{w_adjectives_vulgar.GetRandom(ref random)}");

											if (random.NextBool(0.40f))
											{
												sb.Append($" and {w_defile.GetRandom(ref random)} {w_objects_misc.GetRandom(ref random)}");
											}

											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 13:
										{
											sb.Append($"{w_i_will.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_fight.GetRandom(ref random)}");
											sb.Append(' ');
											sb.Append($"{w_your.GetRandom(ref random)}");
											sb.Append(' ');
											if (random.NextBool(0.50f))
											{
												sb.Append(w_adjectives_vulgar.GetRandom(ref random));
												sb.Append(' ');
											}
											sb.Append($"{w_bodypart.GetRandom(ref random)}");
											if (random.NextBool(0.50f))
											{
												sb.Append(' ');
												sb.Append($"{w_directions.GetRandom(ref random)} ");
											}
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 14:
										{
											sb.Append(w_insert.GetRandom(ref random));
											sb.Append(" a ");
											sb.Append(w_objects_vulgar.GetRandom(ref random));
											sb.Append(" in it");
											if (random.NextBool(0.50f))
											{
												sb.Append(' ');
												if (random.NextBool(0.50f))
												{
													sb.Append(w_adjectives_vulgar.GetRandom(ref random));
													sb.Append(' ');
												}
												sb.Append(w_you.GetRandom(ref random));
												sb.Append(' ');
												sb.Append(w_objects_vulgar.GetRandom(ref random));
											}
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 15:
										{
											sb.Append("get ");
											sb.Append(w_defile.GetRandom(ref random));
											sb.Append(w_ed.GetRandom(ref random));
											sb.Append(' ');
											sb.Append(w_into.GetRandom(ref random));
											sb.Append(' ');
											if (random.NextBool(0.50f))
											{
												if (random.NextBool(0.50f))
												{
													sb.Append(w_your.GetRandom(ref random));
													sb.Append(' ');
												}
												sb.Append(w_adjectives_vulgar.GetRandom(ref random));
												sb.Append(' ');
											}
											sb.Append(w_bodypart.GetRandom(ref random));
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										case 16:
										{
											sb.Append(w_move.GetRandom(ref random));
											sb.Append(' ');
											sb.Append(w_into.GetRandom(ref random));
											sb.Append(' ');
											sb.Append(w_objects_vulgar.GetRandom(ref random));
											sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;

										default:
										{
											if (random.NextBool(0.40f)) sb.Append($"{w_starters.GetRandom(ref random)} ");
											sb.Append($"{w_your.GetRandom(ref random)} is ");
											if (random.NextBool(0.40f)) sb.Append($"{w_targeters.GetRandom(ref random)} ");

											if (random.NextBool(0.40f))
											{
												sb.Append(w_objects_vulgar.GetRandom(ref random));
												if (random.NextBool(0.50f)) sb.Append(w_suffix_vulgar.GetRandom(ref random));
											}
											else
											{
												sb.Append(w_objects_misc.GetRandom(ref random));
											}

											if (random.NextBool(0.20f)) sb.Append($" {w_enders_misc.GetRandom(ref random)}");
											if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										}
										break;
									}

									if (ai_original.anger > 600.00f)
									{
										if (random.NextBool(0.70f)) sb.Append($"{w_enders_upset.GetRandom(ref random)}");
										sb.ToUpperInvariant();
									}

									ai_original.anger -= Maths.Min(ai_original.anger, random.NextFloatRange(50.00f, 1500.00f));

									Sound.Play(ref region, Kobold.snd_insults.GetRandom(ref random), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);

									kobold.next_talk = time + random.NextFloatRange(2.00f, 10.00f - Maths.Clamp(ai_original.anger * 0.02f, 0.00f, 7.00f));
									head_state.t_next_sound = time + 1.50f;
								}
								else if ((time - commandable.last_time) < 0.50f)
								{
									switch (commandable.last_type)
									{
										case Commandable.OrderType.Move:
										{
											var index = random.NextIntRange(0, 2);
											//App.WriteLine(index);

											switch (index)
											{
												//case 0:
												//{
												//	sb.Append(w_me.GetRandom(ref random));
												//	sb.Append(' ');
												//	sb.Append(w_verbs_move.GetRandom(ref random));
												//}
												//break;

												default:
												{
													sb.Append(w_me.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_move.GetRandom(ref random));
													if (random.NextBool(0.25f))
													{
														sb.Append(" like ");
														sb.Append(w_animal.GetRandom(ref random));
													}
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_move.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										case Commandable.OrderType.Harvest:
										{
											var index = random.NextIntRange(0, 2);
											//App.WriteLine(index);

											switch (index)
											{
												default:
												{
													if (random.NextBool(0.50f))
													{
														sb.Append(w_me.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_i_will.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_gather.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(Kobold.GetObjectName(ref random, commandable.last_material_type, commandable.last_layer));
													if (random.NextBool(0.50f))
													{
														sb.Append(' ');
														sb.Append(w_then.GetRandom(ref random));

														sb.Append(' ');
														sb.Append(w_authority.GetRandom(ref random));

														if (random.NextBool(0.50f))
														{
															sb.Append(' ');
															sb.Append("can");
														}

														sb.Append(' ');
														sb.Append(w_buy.GetRandom(ref random));

														sb.Append(' ');
														sb.Append(w_cool.GetRandom(ref random));

														sb.Append(' ');
														sb.Append(w_valuables.GetRandom(ref random));
													}
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_pickup.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										case Commandable.OrderType.Pickup:
										{
											var index = random.NextIntRange(0, 2);
											//App.WriteLine(index);

											switch (index)
											{
												default:
												{
													if (random.NextBool(0.50f))
													{
														sb.Append(w_me.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_i_will.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_take.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(Kobold.GetObjectName(ref random, commandable.last_material_type, commandable.last_layer));
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_move.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										case Commandable.OrderType.Use:
										{
											var index = random.NextIntRange(0, 2);
											//App.WriteLine(index);

											switch (index)
											{
												default:
												{
													if (random.NextBool(0.50f))
													{
														sb.Append(w_me.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_i_will.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_work.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(Kobold.GetObjectName(ref random, commandable.last_material_type, commandable.last_layer));
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_ok.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										case Commandable.OrderType.Attack:
										{
											var index = random.NextIntRange(0, 10);
											//index = 9;
											//App.WriteLine(index);

											switch (index)
											{
												case 0:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_fight.GetRandom(ref random));
												}
												break;

												case 1:
												{
													sb.Append("for");
													sb.Append(' ');
													sb.Append(w_my.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_authority.GetRandom(ref random));
												}
												break;

												case 2:
												{
													sb.Append(w_me.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_fight.GetRandom(ref random));
													if (random.NextBool(0.75f))
													{
														sb.Append(' ');
														sb.Append(w_their.GetRandom(ref random));
													}
													if (random.NextBool(0.25f))
													{
														sb.Append(' ');
														sb.Append(w_adjectives_vulgar.GetRandom(ref random));
													}
													sb.Append(' ');
													if (random.NextBool(0.50f))
													{
														sb.Append(w_family.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_objects_misc.GetRandom(ref random));
													}
												}
												break;

												case 3:
												{
													sb.Append(w_me.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_fight.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_bodypart.GetRandom(ref random));
												}
												break;

												case 4:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_destroy.GetRandom(ref random));
													if (random.NextBool(0.75f))
													{
														sb.Append(' ');
														sb.Append(w_their.GetRandom(ref random));
													}
													if (random.NextBool(0.25f))
													{
														sb.Append(' ');
														sb.Append(w_adjectives.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_structures.GetRandom(ref random));
												}
												break;

												case 5:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_take.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_their.GetRandom(ref random));
													sb.Append(' ');
													if (random.NextBool(0.50f))
													{
														sb.Append(w_family.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_objects_misc.GetRandom(ref random));
													}
													if (random.NextBool(0.50f))
													{
														sb.Append(" and then ");
														sb.Append(w_they.GetRandom(ref random));
														sb.Append(' ');
														sb.Append(w_fear.GetRandom(ref random));
													}
												}
												break;

												case 6:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_insert.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_their.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_bodypart.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_into.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_their.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_family.GetRandom(ref random));
												}
												break;

												case 7:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_destroy.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_they.GetRandom(ref random));
													sb.Append(" with ");
													if (random.NextBool(0.50f))
													{
														sb.Append(w_my.GetRandom(ref random));
														sb.Append(' ');
													}
													sb.Append(w_weapons.GetRandom(ref random));
												}
												break;

												case 8:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_take.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_their.GetRandom(ref random));
													sb.Append(' ');
													if (random.NextBool(0.30f))
													{
														sb.Append(w_family.GetRandom(ref random));
													}
													else if (random.NextBool(0.40f))
													{
														sb.Append(w_valuables.GetRandom(ref random));
													}
													else if (random.NextBool(0.40f))
													{
														sb.Append(w_animal_cute.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_bodypart.GetRandom(ref random));
													}
													if (random.NextBool(0.50f))
													{
														sb.Append(" and ");
														if (random.NextBool(0.50f))
														{
															sb.Append(w_me.GetRandom(ref random));
															sb.Append(' ');
														}
														sb.Append(w_eat.GetRandom(ref random));
														sb.Append(" it");
													}
												}
												break;

												case 9:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_throw.GetRandom(ref random));
													sb.Append(' ');
													if (random.NextBool(0.50f))
													{
														sb.Append(w_their.GetRandom(ref random));
														sb.Append(' ');
													}
													if (random.NextBool(0.75f))
													{
														sb.Append(w_objects_misc.GetRandom(ref random));
														sb.Append(' ');
													}
													else
													{
														sb.Append(w_family.GetRandom(ref random));
														sb.Append(' ');
													}
													sb.Append(w_through.GetRandom(ref random));
													sb.Append(' ');
													if (random.NextBool(0.50f))
													{
														sb.Append(w_their.GetRandom(ref random));
														sb.Append(' ');
													}
													sb.Append("window");
												}
												break;

												default:
												{
													sb.Append(w_i_will.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_fight.GetRandom(ref random));
													if (random.NextBool(0.25f))
													{
														sb.Append(' ');
														sb.Append(w_adjectives_vulgar.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_objects_vulgar.GetRandom(ref random));
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_attack.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										case Commandable.OrderType.Defend:
										{
											var index = random.NextIntRange(0, 3);
											//index = 9;
											//App.WriteLine(index);

											switch (index)
											{
												case 0:
												{
													sb.Append(w_me.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_watch.GetRandom(ref random));
												}
												break;

												case 1:
												{
													sb.Append("for");
													sb.Append(' ');
													sb.Append(w_my.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_authority.GetRandom(ref random));
												}
												break;

												case 2:
												{
													sb.Append(w_me.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_watch.GetRandom(ref random));
													if (random.NextBool(0.75f))
													{
														sb.Append(' ');
														sb.Append(w_authority.GetRandom(ref random));
														sb.Append("'s");
													}
													sb.Append(' ');
													if (random.NextBool(0.50f))
													{
														sb.Append(w_family.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_structures.GetRandom(ref random));
													}
												}
												break;

												default:
												{
													if (random.NextBool(0.50f))
													{
														sb.Append(w_structures.GetRandom(ref random));
													}
													else if (random.NextBool(0.50f))
													{
														sb.Append(w_valuables.GetRandom(ref random));
													}
													else
													{
														sb.Append(w_family.GetRandom(ref random));
													}
													sb.Append(' ');
													sb.Append(w_is.GetRandom(ref random));
													sb.Append(' ');
													sb.Append(w_watch.GetRandom(ref random));
													if (random.NextBool(0.50f))
													{
														sb.Append(w_ed.GetRandom(ref random));
													}
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_ok.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;

										default:
										{
											var index = random.NextIntRange(0, 15);
											//App.WriteLine(index);

											switch (index)
											{
												default:
												{
													sb.Append(w_okay.GetRandom(ref random));
													if (random.NextBool(0.25f))
													{
														sb.Append(' ');
														sb.Append(w_my.GetRandom(ref random));
														sb.Append(' ');
														sb.Append(w_authority.GetRandom(ref random));
													}
												}
												break;
											}

											Sound.Play(ref region, Kobold.snd_ok.GetRandom(), transform.position, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
										}
										break;
									}

									kobold.next_talk = time + random.NextFloatRange(3.00f, 15.00f);
									head_state.t_next_sound = time + 0.50f;
								}
								else
								{
									var index = random.NextIntRange(0, 13);
									//index = 12;

									switch (index)
									{
										// wondering about food
										case 0:
										{
											if (random.NextBool(0.40f))
											{
												if (random.NextBool(0.40f)) sb.AppendSuffixed(w_us.GetRandom(ref random));
												else if (random.NextBool(0.40f)) sb.AppendSuffixed(w_you.GetRandom(ref random));
												else sb.AppendSuffixed(w_me.GetRandom(ref random));

												sb.AppendSuffixed(w_learn.GetRandom(ref random));
											}

											sb.Append(w_what.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_us.GetRandom(ref random));
											else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_you.GetRandom(ref random));
											else sb.AppendPrefixed(w_me.GetRandom(ref random));

											sb.AppendPrefixed(w_eat.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.AppendPrefixed(w_time.GetRandom(ref random));
											sb.AppendPrefixed(w_for.GetRandom(ref random));
											sb.AppendPrefixed(w_supper.GetRandom(ref random));

											if (random.NextBool(0.70f)) sb.Append(w_enders_curious.GetRandom(ref random));
											else if (random.NextBool(0.20f)) sb.Append(w_enders_upset.GetRandom(ref random));
										}
										break;

										// wants to beat the shit out of something
										case 1:
										{
											sb.Append(w_me.GetRandom(ref random));
											sb.AppendPrefixed(w_want.GetRandom(ref random));
											sb.AppendPrefixed(w_fight.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.Append(w_enders_misc.GetRandom(ref random));
										}
										break;

										// complaining about someone wronging them
										case 2:
										{
											if (random.NextBool(0.40f)) sb.AppendSuffixed(w_adjectives_vulgar.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendSuffixed(w_adjectives.GetRandom(ref random));

											sb.Append(w_enemy.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_destroy.GetRandom(ref random));
											else sb.AppendPrefixed(w_take.GetRandom(ref random));
											sb.Append(w_ed.GetRandom(ref random));

											sb.AppendPrefixed(w_our.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_structures.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_valuables.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_objects_misc.GetRandom(ref random));
											else sb.AppendPrefixed(w_comrades.GetRandom(ref random));

											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_because.GetRandom(ref random));
												sb.AppendPrefixed(w_they.GetRandom(ref random));
												sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));
												if (random.NextBool(0.80f)) sb.AppendPrefixed(w_as.GetRandom(ref random));
												sb.AppendPrefixed(w_objects_vulgar.GetRandom(ref random));
											}

											if (random.NextBool(0.80f)) sb.Append(w_enders_upset.GetRandom(ref random));
										}
										break;

										// wants wealth
										case 3:
										{
											sb.Append(w_me.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_want.GetRandom(ref random));
											else if (random.NextBool(0.50f)) sb.AppendPrefixed(w_gather.GetRandom(ref random));
											else sb.AppendPrefixed(w_take.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));
											sb.AppendPrefixed(w_valuables.GetRandom(ref random));
											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_because.GetRandom(ref random));
												if (random.NextBool(0.40f)) sb.AppendPrefixed(w_they.GetRandom(ref random));
												sb.AppendPrefixed(w_cool.GetRandom(ref random));
												if (random.NextBool(0.40f))
												{
													sb.AppendPrefixed(w_and.GetRandom(ref random));
													if (random.NextBool(0.40f))
													{
														if (random.NextBool(0.40f)) sb.AppendPrefixed(w_comrades.GetRandom(ref random));
														else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_family.GetRandom(ref random));
														else sb.AppendPrefixed(w_authority.GetRandom(ref random));

														if (random.NextBool(0.10f))
														{
															sb.Append(w_enders_nevermind.GetRandom(ref random));
															break;
														}

														sb.AppendPrefixed(w_have.GetRandom(ref random));
														sb.AppendPrefixed(w_it.GetRandom(ref random));
														if (random.NextBool(0.50f))
														{
															sb.AppendPrefixed(w_but.GetRandom(ref random));
															sb.AppendPrefixed(w_is.GetRandom(ref random));
															sb.AppendPrefixed(w_bad.GetRandom(ref random));
														}
														else if (random.NextBool(0.50f))
														{
															sb.AppendPrefixed(w_because.GetRandom(ref random));
															sb.AppendPrefixed(w_they.GetRandom(ref random));
															if (random.NextBool(0.50f))
															{
																sb.AppendPrefixed(w_have.GetRandom(ref random));
																sb.AppendPrefixed(w_it.GetRandom(ref random));
															}
															else
															{
																if (random.NextBool(0.40f)) sb.AppendPrefixed(w_take.GetRandom(ref random));
																else if (random.NextBool(0.50f)) sb.AppendPrefixed(w_work.GetRandom(ref random));
																else sb.AppendPrefixed(w_cool.GetRandom(ref random));

																if (random.NextBool(0.40f))
																{
																	sb.Append(w_enders_low.GetRandom(ref random));

																	if (random.NextBool(0.30f))
																	{
																		sb.AppendPrefixed(w_objects_vulgar.GetRandom(ref random));
																		sb.AppendPrefixed(w_suffix_vulgar.GetRandom(ref random));

																		if (random.NextBool(0.80f)) sb.Append(w_enders_upset.GetRandom(ref random));
																	}
																}
															}
														}
													}
													else
													{
														sb.AppendPrefixed(w_i_will.GetRandom(ref random));
														sb.AppendPrefixed(w_become.GetRandom(ref random));

														if (random.NextBool(0.40f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));
														else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_authority.GetRandom(ref random));
														else sb.AppendPrefixed(w_they.GetRandom(ref random));

														if (random.NextBool(0.50f)) sb.Append(w_enders_upset.GetRandom(ref random));
														else if (random.NextBool(0.50f)) sb.Append(w_enders_misc.GetRandom(ref random));
													}
												}
											}
											else
											{
												if (random.NextBool(0.50f)) sb.Append(w_enders_misc.GetRandom(ref random));
											}
										}
										break;

										// wants more stuff
										case 4:
										{
											sb.Append(w_me.GetRandom(ref random));

											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_and.GetRandom(ref random));

												if (random.NextBool(0.30f)) sb.AppendPrefixed(w_our.GetRandom(ref random));
												else if (random.NextBool(0.50f)) sb.AppendPrefixed(w_my.GetRandom(ref random));

												if (random.NextBool(0.30f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));
												else if (random.NextBool(0.15f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));

												if (random.NextBool(0.50f)) sb.AppendPrefixed(w_comrades.GetRandom(ref random));
												else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_us.GetRandom(ref random));
												else sb.AppendPrefixed(w_authority.GetRandom(ref random));
											}
											sb.AppendPrefixed(w_want.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_have.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_take.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_gather.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_buy.GetRandom(ref random));

											if (random.NextBool(0.70f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));

											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_weapons.GetRandom(ref random));
											else if (random.NextBool(0.45f)) sb.AppendPrefixed(w_valuables.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_food.GetRandom(ref random));
											else sb.AppendPrefixed(w_devices.GetRandom(ref random));

											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_and.GetRandom(ref random));

												if (random.NextBool(0.40f))
												{
													sb.AppendPrefixed(w_throw.GetRandom(ref random));
													if (random.NextBool(0.60f)) sb.AppendPrefixed(w_at.GetRandom(ref random));
													else sb.AppendPrefixed(w_through.GetRandom(ref random));
												}
												else if (random.NextBool(0.45f)) sb.AppendPrefixed(w_fight.GetRandom(ref random));
												else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_curse.GetRandom(ref random));
												else sb.AppendPrefixed(w_take.GetRandom(ref random));

												if (random.NextBool(0.40f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
												else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
												else sb.AppendPrefixed(w_objects_misc.GetRandom(ref random));

												if (random.NextBool(0.50f)) sb.Append(w_enders_upset.GetRandom(ref random));
												else sb.Append(w_enders_misc.GetRandom(ref random));
											}
											else
											{
												if (random.NextBool(0.70f)) sb.Append(w_enders_misc.GetRandom(ref random));
												else sb.Append(w_enders_nevermind.GetRandom(ref random));
											}
										}
										break;

										// curious about inner workings of something 
										case 5:
										{
											sb.Append(w_question.GetRandom(ref random));
											if (random.NextBool(0.80f)) sb.AppendPrefixed(w_is.GetRandom(ref random));
											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_objects_misc.GetRandom(ref random));
											else sb.AppendPrefixed(w_family.GetRandom(ref random));
											if (random.NextBool(0.50f)) sb.AppendPrefixed(w_work.GetRandom(ref random));
											else sb.AppendPrefixed(w_move.GetRandom(ref random));
											sb.Append(w_enders_curious.GetRandom(ref random));
										}
										break;

										// wondering about meals and eating
										case 6:
										{
											if (random.NextBool(0.70f)) sb.AppendSuffixed(w_me.GetRandom(ref random));
											else sb.AppendSuffixed(w_us.GetRandom(ref random));
											sb.Append(w_eat.GetRandom(ref random));
											if (random.NextBool(0.30f)) sb.AppendPrefixed(w_adjectives.GetRandom(ref random));
											else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));
											sb.AppendPrefixed(w_food.GetRandom(ref random));
											if (random.NextBool(0.60f))
											{
												sb.AppendPrefixed(w_then.GetRandom(ref random));
												sb.AppendPrefixed(w_food.GetRandom(ref random));
											}
											if (random.NextBool(0.50f)) sb.AppendPrefixed(w_time.GetRandom(ref random));
											if (random.NextBool(0.70f)) sb.AppendPrefixed("mmmmm");
											if (random.NextBool(0.70f)) sb.Append(w_enders_misc.GetRandom(ref random));
										}
										break;

										// talks about diseases
										case 7:
										{
											if (random.NextBool(0.20f)) sb.AppendSuffixed(w_starters.GetRandom(ref random));
											else if (random.NextBool(0.20f)) sb.AppendSuffixed(w_curse.GetRandom(ref random));

											sb.Append(w_me.GetRandom(ref random));

											if (random.NextBool(0.80f))
											{
												switch (random.NextUIntRange(0, 4))
												{
													default:
													case 0: sb.AppendPrefixed(w_bodypart.GetRandom(ref random)); break;
													case 1: sb.AppendPrefixed(w_family.GetRandom(ref random)); break;
													case 2: sb.AppendPrefixed(w_animal.GetRandom(ref random)); break;
													case 3: sb.AppendPrefixed(w_authority.GetRandom(ref random)); break;
												}
											}

											if (random.NextBool(0.20f)) sb.AppendPrefixed(w_is.GetRandom(ref random));
											else if (random.NextBool(0.25f)) sb.AppendPrefixed(w_have.GetRandom(ref random));
											else if (random.NextBool(0.22f)) sb.AppendPrefixed(w_through.GetRandom(ref random));
											else
											{
												sb.AppendPrefixed(w_fight.GetRandom(ref random));
												sb.Append(w_ed.GetRandom(ref random));

												if (random.NextBool(0.40f)) sb.AppendPrefixed(w_because.GetRandom(ref random));
											}

											if (random.NextBool(0.30f)) sb.AppendPrefixed(w_adjectives.GetRandom(ref random));
											if (random.NextBool(0.20f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));

											sb.AppendPrefixed(w_diseases.GetRandom(ref random));

											if (random.NextBool(0.76f))
											{
												sb.Append(w_enders_low.GetRandom(ref random));
												if (random.NextBool(0.40f))
												{
													sb.AppendPrefixed(w_is.GetRandom(ref random));
													if (random.NextBool(0.25f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));

													if (random.NextBool(0.40f))
													{
														if (random.NextBool(0.60f)) sb.AppendPrefixed(w_discomfort.GetRandom(ref random));
														else sb.AppendPrefixed(w_curse.GetRandom(ref random));

														if (random.NextBool(0.60f)) sb.Append(w_enders_low.GetRandom(ref random));
														else if (random.NextBool(0.40f)) sb.Append(w_enders_curious.GetRandom(ref random));
														else sb.Append(w_enders_upset.GetRandom(ref random));
													}
													else
													{
														if (random.NextBool(0.30f)) sb.AppendPrefixed(w_okay.GetRandom(ref random));
														else sb.AppendPrefixed(w_discomfort.GetRandom(ref random));
													}
												}
											}
											else
											{
												sb.Append(w_enders_misc.GetRandom(ref random));
											}
										}
										break;

										// abstract kobold philosophy
										case 8:
										{
											if (random.NextBool(0.30f)) sb.Append(w_because.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.Append(w_into.GetRandom(ref random));
											else sb.Append(w_i_am.GetRandom(ref random));

											if (random.NextBool(0.10f)) sb.AppendPrefixed(w_curse.GetRandom(ref random));
											else if (random.NextBool(0.10f)) sb.AppendPrefixed(w_time.GetRandom(ref random));
											else if (random.NextBool(0.10f)) sb.AppendPrefixed(w_through.GetRandom(ref random));

											sb.AppendPrefixed(w_then.GetRandom(ref random));

											if (random.NextBool(0.30f)) sb.AppendPrefixed(w_us.GetRandom(ref random));
											else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_is.GetRandom(ref random));
											else sb.AppendPrefixed(w_i_will.GetRandom(ref random));

											if (random.NextBool(0.20f)) sb.AppendPrefixed(w_fear.GetRandom(ref random));
											else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_move.GetRandom(ref random));

											if (random.NextBool(0.70f))
											{
												sb.Append(w_enders_low.GetRandom(ref random));

												// sudden realization
												if (random.NextBool(0.10f))
												{
													sb.AppendPrefixed(w_curse.GetRandom(ref random));
													if (random.NextBool(0.40f)) sb.Append(w_enders_upset.GetRandom(ref random));
												}
											}
											else
											{
												sb.Append(w_enders_misc.GetRandom(ref random));
											}
										}
										break;

										// rants about some discomfort
										case 9:
										{
											sb.Append(w_my.GetRandom(ref random));
											if (random.NextBool(0.40f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));
											sb.AppendPrefixed(w_bodypart.GetRandom(ref random));
											sb.AppendPrefixed(w_discomfort.GetRandom(ref random));
											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_as.GetRandom(ref random));
												if (random.NextBool(0.20f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));
												sb.AppendPrefixed(w_objects_vulgar.GetRandom(ref random));
											}
											if (random.NextBool(0.50f)) sb.Append(w_enders_low.GetRandom(ref random));
											if (random.NextBool(0.30f)) sb.Append(w_enders_misc.GetRandom(ref random));
										}
										break;

										// gossips
										default:
										{
											if (random.NextBool(0.30f))
											{
												sb.Append(w_me.GetRandom(ref random));

												sb.AppendPrefixed(w_hear.GetRandom(ref random)); 
												sb.Append(w_ed.GetRandom(ref random));
											}
											else
											{
												sb.Append(w_my.GetRandom(ref random));

												if (random.NextBool(0.47f))
												{
													sb.AppendPrefixed(w_comrades.GetRandom(ref random));
													sb.Append("'s");
												}

												if (random.NextBool(0.40f)) sb.AppendPrefixed(w_comrades.GetRandom(ref random));
												else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_authority.GetRandom(ref random));
												else sb.AppendPrefixed(w_infosource.GetRandom(ref random));

												if (random.NextBool(0.50f))
												{
													sb.AppendPrefixed(w_hear.GetRandom(ref random));
													sb.Append(w_ed.GetRandom(ref random));

													if (random.NextBool(0.40f)) sb.AppendPrefixed(w_that.GetRandom(ref random));

													if (random.NextBool(0.20f)) sb.AppendPrefixed(w_authority.GetRandom(ref random));
													else sb.AppendPrefixed(w_infosource.GetRandom(ref random));

													sb.AppendPrefixed(w_say.GetRandom(ref random));
													sb.Append(w_ed.GetRandom(ref random));
												}
												else
												{
													sb.AppendPrefixed(w_say.GetRandom(ref random));
													sb.Append(w_ed.GetRandom(ref random));
												}
											}

											if (random.NextBool(0.22f)) sb.AppendSuffixed(w_enders_low.GetRandom(ref random));

											sb.AppendPrefixed(w_that.GetRandom(ref random));

											if (random.NextBool(0.25f)) sb.AppendSuffixed(w_enders_low.GetRandom(ref random));

											if (random.NextBool(0.24f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));
											else if (random.NextBool(0.22f))
											{
												sb.AppendPrefixed(w_diseases.GetRandom(ref random));
												sb.Append(w_ed.GetRandom(ref random));
											}
											else if (random.NextBool(0.24f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));
											else if (random.NextBool(0.38f)) sb.AppendPrefixed(w_bad.GetRandom(ref random));
											else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_adjectives.GetRandom(ref random));

											if (random.NextBool(0.10f)) sb.AppendSuffixed(w_enders_low.GetRandom(ref random));							

											if (random.NextBool(0.24f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
											else if (random.NextBool(0.32f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
											else if (random.NextBool(0.24f))
											{
												sb.AppendPrefixed(w_authority.GetRandom(ref random));

												if (random.NextBool(0.32f))
												{
													sb.Append(w_enders_nevermind.GetRandom(ref random));
													break;
												}
											}
											else if (random.NextBool(0.48f)) sb.AppendPrefixed(w_bodypart.GetRandom(ref random));
											else sb.AppendPrefixed(w_us.GetRandom(ref random));

											if (random.NextBool(0.25f)) sb.AppendSuffixed(w_enders_low.GetRandom(ref random));

											if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_have.GetRandom(ref random));

												if (random.NextBool(0.44f)) sb.AppendPrefixed(w_cool.GetRandom(ref random));
												else if (random.NextBool(0.48f)) sb.AppendPrefixed(w_bad.GetRandom(ref random));
												else sb.AppendPrefixed(w_adjectives.GetRandom(ref random));

												if (random.NextBool(0.10f)) sb.AppendSuffixed(w_enders_low.GetRandom(ref random));
												
												if (random.NextBool(0.40f)) sb.AppendPrefixed(w_food.GetRandom(ref random));
												else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_weapons.GetRandom(ref random));
												else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_structures.GetRandom(ref random));
												else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
												else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_devices.GetRandom(ref random));
												else sb.AppendPrefixed(w_bodypart.GetRandom(ref random));
											}
											else if (random.NextBool(0.40f))
											{
												sb.AppendPrefixed(w_eat.GetRandom(ref random));

												if (random.NextBool(0.10f)) sb.AppendPrefixed(w_adjectives.GetRandom(ref random));
												else if (random.NextBool(0.10f)) sb.AppendPrefixed(w_adjectives_vulgar.GetRandom(ref random));

												if (random.NextBool(0.25f)) sb.AppendPrefixed(w_food.GetRandom(ref random));
												else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
												else if (random.NextBool(0.24f)) sb.AppendPrefixed(w_supper.GetRandom(ref random));
												else if (random.NextBool(0.22f)) sb.AppendPrefixed(w_comrades.GetRandom(ref random));
												else if (random.NextBool(0.32f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
												else if (random.NextBool(0.22f)) sb.AppendPrefixed(w_bodypart.GetRandom(ref random));
												else sb.AppendPrefixed(w_family.GetRandom(ref random));
											}
											else // if (random.NextBool(0.20f))
											{
												sb.AppendPrefixed(w_become.GetRandom(ref random));

												if (random.NextBool(0.32f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
												else if (random.NextBool(0.22f)) sb.AppendPrefixed(w_authority.GetRandom(ref random));
												else if (random.NextBool(0.32f)) sb.AppendPrefixed(w_objects_vulgar.GetRandom(ref random));
												else sb.AppendPrefixed(w_animal.GetRandom(ref random));
											}
											//else if (random.NextBool(0.20f))
											//{
											//	sb.AppendPrefixed(w_work.GetRandom(ref random));

											//	if (random.NextBool(0.30f)) sb.AppendPrefixed(w_at.GetRandom(ref random));
											//	else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_as.GetRandom(ref random));

											//	if (random.NextBool(0.20f)) sb.AppendPrefixed(w_structures.GetRandom(ref random));
											//	else if (random.NextBool(0.15f)) sb.AppendPrefixed(w_authority.GetRandom(ref random));
											//	else if (random.NextBool(0.14f)) sb.AppendPrefixed(w_animal_big.GetRandom(ref random));
											//	else sb.AppendPrefixed(w_enemy.GetRandom(ref random));
											//}
											//else
											//{
											//	if (random.NextBool(0.30f)) sb.AppendPrefixed(w_insert.GetRandom(ref random));
											//	else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_throw.GetRandom(ref random));
											//	else if (random.NextBool(0.30f)) sb.AppendPrefixed(w_move.GetRandom(ref random));
											//	else sb.AppendPrefixed(w_defile.GetRandom(ref random));

											//	if (random.NextBool(0.20f)) sb.AppendPrefixed(w_food.GetRandom(ref random));
											//	else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_family.GetRandom(ref random));
											//	else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_animal.GetRandom(ref random));
											//	else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_animal_big.GetRandom(ref random));
											//	else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
											//	else sb.AppendPrefixed(w_comrades.GetRandom(ref random));

											//	if (random.NextBool(0.30f)) sb.AppendPrefixed(w_at.GetRandom(ref random));
											//	else if (random.NextBool(0.40f)) sb.AppendPrefixed(w_into.GetRandom(ref random));
											//	else sb.AppendPrefixed(w_directions.GetRandom(ref random));

											//	if (random.NextBool(0.30f)) sb.AppendPrefixed(w_enemy.GetRandom(ref random));
											//	else if (random.NextBool(0.20f)) sb.AppendPrefixed(w_structures.GetRandom(ref random));
											//	else sb.AppendPrefixed(w_devices.GetRandom(ref random));
											//}

											if (random.NextBool(0.30f)) sb.Append(w_enders_low.GetRandom(ref random));

											if (random.NextBool(0.35f)) sb.Append(w_enders_curious.GetRandom(ref random));
											else if (random.NextBool(0.35f)) sb.Append(w_enders_upset.GetRandom(ref random));
											else if (random.NextBool(0.35f)) sb.Append(w_enders_misc.GetRandom(ref random));
											else sb.Append(w_enders_low.GetRandom(ref random));
										}
										break;
									}

									kobold.next_talk = time + random.NextFloatRange(10.00f, 30.00f);
									head_state.t_next_sound = time + 0.50f;
								}

								var text = sb.ToString().AsSpan().Trim();
								//App.WriteLine($"\"{text}\" -- Some kobold", App.Color.DarkGray, show_stacktrace: false);

								speech_bubble.text = text;
								speech_bubble.Sync(ent_speech_bubble, true);

								//ai_original.anger -= Maths.Min(ai_original.anger, random.NextFloatRange(50.00f, 300.00f));
							}
						}
					}
				}
			}
		}
#endif
	}
}
