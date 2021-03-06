//
// System.ComponentModel.MaskedTextProvider test cases
//
// Authors:
// 	Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// (c) 2007 Novell, Inc.
//


// a reference like this is required: 
// -reference:MS_System=<gac>\System.dll
extern alias MS_System;
using System;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using MS_System_ComponentModel = MS_System.System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	class MaskedTextProviderTestGenerator
	{
		const int MAXFAILEDTESTS = 100;
		static string [] test_masks = new string [] { 
		// Misc
		"abc", 
		"aba",
		"abaa",
		"a?b?c" ,
		"09#L?&CAa.,:/$<>|\\\\",
		// Social security numbers
		"000-00-0000", "0 00 00 00 000 000 00",	"000-00-0000", "000000-0000000",
		// Zip codes
		"00000-9999", "00000", "000-0000", "99000",
		// Dates
		"00/00/0000", "00 /00 /0000",  "00 /00 /0000 00:00",  "00/00/0000 00:00", "0000-00-00 90:00:00",
		"0000-00-00 90:00", "0000-00-00", "00->L<LL-0000", "90:00", "00:00",
		// Phone numbers
		"(999)-000-0000", "00000", "99999", "00 00 00 00 00 00", "0000 00000", "99900-9990-0000", "(900)9000-0000",
		"(00)9000-0000", "(999)9000-0000", "000-0000", "9000-0000",
		// Money
		"$999,999.00"
		};

		static int tab = 0;
		static StreamWriter writer;
		static bool dont_write;
		static char [] char_values = new char [] { char.MinValue, char.MaxValue, 'a', '/', ' ', '*', '1'};
		static int [] int_values = new int [] { int.MinValue, -1, 0, 1, int.MaxValue };
		static string [] string_values = new string [] { null, string.Empty, "a", "a longer string value", new string ('z', 1024) };
		static MaskedTextResultHint [] hint_values = new MaskedTextResultHint [] { MaskedTextResultHint.AlphanumericCharacterExpected, MaskedTextResultHint.AsciiCharacterExpected, MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint.DigitExpected, MaskedTextResultHint.InvalidInput, MaskedTextResultHint.LetterExpected, MaskedTextResultHint.NoEffect, MaskedTextResultHint.NonEditPosition, MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint.PromptCharNotAllowed, MaskedTextResultHint.SideEffect, MaskedTextResultHint.SignedDigitExpected, MaskedTextResultHint.Success, MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint.Unknown, (MaskedTextResultHint)(-1) };
		static CultureInfo [] culture_infos = new CultureInfo [] { null, CultureInfo.CurrentCulture, CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo ("es-ES") };
		static object [] object_values = new object [] { "a", 1 };
		static Type type_Mono;
		static Type type_MS;
		static Type type_Hint_Mono;
		static Type type_Hint_MS;
		static Type type = typeof (MaskedTextProvider);
		static ConstructorInfo [] ctors = type.GetConstructors ();
		static MethodInfo [] methods = type.GetMethods ();
		static PropertyInfo [] props = type.GetProperties ();

		static List<ConstructorInfo> ok_constructors = new List<ConstructorInfo> ();
		static List<string> ok_constructors_statements = new List<string> ();
		static List<object []> ok_constructors_args = new List<object []> ();

		static char [] [] add_char_test_values = new char [] [] {
			new char [] {char.MinValue, char.MaxValue, 'A', '1', '+', '*', '8', '?', '@', 'A', 'Z', '??' },
			new char [] {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' },
			new char [] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'},
			// Some randomly generated characters.
			new char [] {'\x26CC', '\xFE68', '\xB6D4', '\x8D66', '\xE776', '\x786F', '\x78E9', '\x12E4', '\x1B02', '\xFFC2', '\x5846', '\xF686', '\x438B', '\x2DE2'}, 
			new char [] {'\x5B83', '\xC320', '\x570', '\xE07A', '\xD42D', '\xF21C', '\xEA4', '\x1113', '\x2851', '\x2926', '\x706D', '\xD59E', '\x8CCD', '\xC5DF', '\x7223', '\x7F75'}, 
			new char [] {'\xC5E6', '\x5FE2', '\x61C0', '\xAB57', '\x8C1', '\x50D0', '\xCE1B', '\xABBF', '\xB7C7', '\xDB6F', '\x2DC3', '\xCF99'}, 
			new char [] {'\x89A1', '\xB987', '\xD18D', '\x727E', '\x35BE', '\x19EF', '\x6D02', '\xF4A5', '\x79F4', '\xC7A0', '\x1827', '\xED54', '\x8E82', '\x643F', '\x7709', '\xA2D0', '\xEC1B', '\x4D04'}, 
			new char [] {'\x804C', '\xB3AA', '\x309F', '\xE3A8', '\xCC22', '\x217C', '\x52C1', '\x7250', '\x3754', '\x34BB', '\x1C65', '\x16AC', '\xE0E1'}, 
			new char [] {'\x4088', '\x9F85', '\xB6E5', '\x411', '\x1A4F'}, 
			new char [] {'\x5833', '\x1273', '\xAFF6', '\x4BF2', '\x9841', '\x4998', '\xBE02', '\x7A3E', '\xEC91', '\x5712', '\x8EE'}, 
			new char [] {'\x8E6A', '\x84E4', '\x4F4C', '\x341E', '\x5901', '\xD5DE', '\x56B', '\x5101', '\xE2FC', '\xA79F', '\x35AD', '\xBFE9', '\x5D8E', '\xB0F4', '\x3746'}, 
			new char [] {'\x4DFA', '\xC4BA', '\xC023', '\x9EBE', '\xD1CC', '\xBCE3', '\x50AB', '\x6DD9', '\x3B3', '\xE4AD', '\x4B66', '\x8289', '\x6379'}, 
			new char [] {'\x85BF', '\xE041', '\x2BCC', '\x50BA', '\x8842', '\x5BFD', '\xF22E', '\xC6A', '\x4684', '\xE106', '\xFEA6', '\xC94D', '\xAD24', '\xB093', '\xDCC6', '\xF00D'}, 
			new char [] {'\xE74D', '\x1252', '\x1228', '\x2C44', '\x27D6', '\x96EF', '\x6A2F', '\xF9DE', '\xD186', '\x3438', '\xE173', '\x306A', '\x7453', '\x8A77', '\x82E1', '\xED88', '\xA79', '\x21E0'}, 
			new char [] {'\x941C', '\xCD3', '\x28B1', '\xDB49', '\xB9AB', '\x418F'}, 
			new char [] {'\xDF1C', '\xA018', '\x87F', '\xFBF', '\xA018', '\x9112', '\x13A6', '\xF64A', '\x6418'}, 
			new char [] {'\x5150', '\xAC3E', '\x5DE8', '\x4952', '\xC19D', '\x56DC', '\xB6BB', '\x27C5'}, 
			new char [] {'\xDBFF'}, 
			new char [] {'\x568E', '\x7BC4', '\xDBC4', '\xA2AA', '\x8EB2', '\x875A', '\x5BF0', '\xE18F', '\xBE9B', '\x3709', '\x587C', '\xEAB4', '\xA9A0', '\xB7D2', '\xCA17', '\xF15F'}, 
			new char [] {'\x941F', '\x5060', '\x1CC4', '\x7E09', '\x265E', '\x12AA', '\x9C37', '\x5E3B', '\xC3F8', '\xC19', '\xD27F', '\xB5F7', '\x71F6', '\xB383', '\xA8F1'}, 
			new char [] {'\xAC05', '\x888D', '\x2453', '\x2CBA', '\x6D14', '\x1165', '\x9B8'}, 
			new char [] {'\x14BA', '\xA57D', '\x392E', '\xF8D3', '\xC189', '\xB447', '\x917F', '\xF786', '\x657C', '\xF4F3', '\x93A5', '\xC05A', '\xBF3B', '\x5427'}, 
			new char [] {'\x691A', '\x3F8C', '\x446F'}, 
			new char [] {'\x7B40', '\x8970', '\x2B97', '\x4CA5', '\x8385', '\xAF8B', '\x8524', '\xFD9A', '\x2F45', '\xA5C3', '\xC4A4', '\x54B2', '\x82BA', '\x46A7', '\x650D'}, 
			new char [] {'\x80E1', '\xD97B', '\xA363', '\x9CB1', '\xFB0', '\x7A9A', '\xDAF9', '\x507A'}, 
			new char [] {'\x1FDE', '\xE896', '\xA655', '\x57DE', '\x585D', '\xBB39', '\xED2D', '\x28A5', '\x46AA'}, 
			new char [] {'\x2E45', '\xE923', '\x58D8', '\xB5A9', '\x4948', '\x3C65', '\xB6AC', '\x623A', '\x51C2', '\xA0D8', '\xA041'}, 
			new char [] {'\x79B3'}
		};
		static int [] add_char_test_char_count = new int [] { 1, 4, 7, 9, 6, 4, 3, 2 };
		static string destination_file;

		static string [] [] add_string_test_values = new string [] [] {
			new string [] {"", "a", "abc", "`pasdf", "1297.1243,5132", "1", "???", "%", "$", "123", "AMKHJ"},
			new string [] {},
			// Some random data here too.
			new string [] {@"??????????????????????????????????????????????????????????????????????????????????", @"????????????", @"????????????????????????????????????????????????????????", @"???????????????????????????????????????????", @"??????????????????????????????", @"???????????????????????????????????????????????????????????????????", @"??????????????????????????????????????????????????????????????????????????????????????????", @"????????????????????????", @"????????????????????????????????????????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????"}, 
			new string [] {@"??????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????", @"???????????????????????????????????????????????????????????", @"????????????????????????????????????????????????????", @"?????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????????????????????????", @"?????????????????????????????????????????????????????????????????????????????????????????????", @"????????????????????????????????????????????????", @"???????????????????????????????????????????????????????????????", @"??????????????????????????????????????????", @"???????????????????????????????????????????????????????????????", @"????????????????????????????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????", @"????????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????<???????????????????????????????????????", @"??????????????????????????????????????????????????????????????????????????????????????????????", @"??????????????????????????????????????????????????"}, 
			new string [] {@"??[zZL;_????????0??XXZ", @"????o ??V????????????R??@V", @"????<??Xq57??zoD??????%??<????A??Jw$??9??l??????>3<'w??%??M????g5(", @"r??J????H??IP^W????t??????""??F", @"dM??iO-????????d)??I.z??", @"Z??u??CUD+????\$????]??7]????", @"??????????V??K\$fP??u????@??????F????wkI_??iR????Tb4=??????Q0??", @"z????)????w@dBt??????D/??/F??M\??s??????9E(??V}??<|t5Vw", @"A??????b*1w????R????????0FEq6??I lq??_t7V??s????????E@G??&0??`", @"\A??????MO5${??u??3y??+_yM31??^??M9????????'-????g`;??-N:e", @"???????? r??c;]Orn??N??????????????""??}G??A??q", @"Q??x*????????????d;??K??????????:????????????u%??zX9<<??2iD????????????22", @"T0????G.????s??r????m????#5??????;??HF7", @"??"}, 
			new string [] {@"n??????3euj|!??b??Op@????i??X+^QiH??K????YL????z??s", @"e??#??E??~????~-:P", @"%????N????", @"&??????g-o^Z??2????c_????", @"N~mi7??S????l??????????L????'V??", @"??bj??3??YQ)????7M????T????&??????7????WhB??VX9??_;i??&l????)", @"??????????>????????\??ppKm????????N??x??Xd(????R??@????????wE", @"Y'????A????'>L\H????r????@)????:wQ=_????Sb??iJv????F??9&=????", @"??????R????????.????????a??Z%????m??H??,??t^????rZ", @"??+??l@????-A????""??D??Yr??", @"PM|V.??", @"=zugL??IB????D????i????4z????+??Y??????g??%??b{7=??qJ", @"??????]?????????H:lOU??gY??????", @"[]v??`????=W??nP??(w??3??????k|???????Y??????", @"[??V??K??????????-hv????????o????CHt??????????z??sR????X??@??>??`e????(??????:"}, 
			new string [] {@"??????1??YLG????f????V??????????", @"??N??p??T??r9??8??nX6??????????nJ??i??????", @"??9C??????Pjy????m??d????????Ry??y??p??????VD??K????"}, 
			new string [] {@"OR??i????SD??w??ttyX????Nip??3i????J??N59??0??y", @"??68UBn????L????OVKYO??????????jU??E??uFz", @"Gt????n??W????W????iM7biQDI??w????????", @"d????????6????????i??6????jp????yzp??????????x????4O????Y"}, 
			new string [] {@"v??A??????3", @"??m????????8????2??v9", @"WN??X????udC??M??0??5??????J??U7i7L????????o????r??????y????????????", @"LzPZ??o????j????", @"??O??d31??Z8B????y????nfSDz??v6????????w??W??????AAHyj????", @"??B??xiE????L????9mmHq??", @"??W2RMw", @"????????n??JD????K????5kZ??????????4ct??z??????", @"??a??iw??d????HM??I2????2??4??SP", @"W252????s????SD??F", @"R????wX??rsu??v??b3w??????u??G??S5??7??B??v??YG??Y??", @"F??????mwQ??MP????z??8??1??C????rg??????F??Z??????mx??OY??????2V??pC??62", @"??", @"S????zhm??????C3oB????Qh??0s4h??Oj????????", @"??XR????O6??", @"??tq??c????V??C??8??7??WGw5??p??0????????Y??", @"??i??????1n??kf??????61????????????8????????u??W????????7y??X1s????q9i??", @"??????????VQ??????????????G"}, 
			new string [] {@"??84my??????NV??E????m????EtQD??0Y3??n??s????K????t????????", @"i????????Sn????k??????Btu9z??p????wi??kh??", @"????G????7??cA????86??fj2zg??V????3??85????J??3gq????s??JC", @"??c??J????r??oKUI73??2??V??7??xO??????????E??T??6????d??V????L??7tl", @"ah??3V????p??tW??q??lM??B??NaM??z??QT??", @"??G??w??7??Z??4??u??QOP??l????????NM??tDh", @"X??????g??oL????????7????r??Abd????ju????ygB????????DJ??????8????"}, 
			new string [] {@"??7??p??Y??J??wOnqpW9", @"????a????S????S??jdr5????sXB????M??s0D??eGR??w??????h13v??", @"????????VA??gcu3C????????zKYY????K??km????Qzn??7XAy"}, 
			new string [] {@"??A????????????????????UL??xtK????V????O??????K??1????????p????W????F??????0F??h", @"n??6??E", @"q????rY??5z6??koiD????0q", @"??vm6Vz??ItX????????i??i??ZHK0??1LK9Z", @"FU??", @"Y????X??I??A??GP??????Lk????????????F", @"x7????b????V??x????O??9Mi??8??0b????E", @"??????7??MT????t??g????sw??iB6????????n??????????l8", @"??b????????????????????m????6??????AI5??H??4o????k????8????????S??", @"????????G????y??Za??????????K????H??oo", @"??L??9????tPASJ??rW??ca??????????X??e????84rGXmvxeN??hT??hV", @"s??Gq????????xY??????rt??????z????????????????w", @"WvjH????????g??b??Ok????????????", @"????pjZ??????Y??h??r????", @"????????????????34cpCs??????????????t????????5??n"}, 
			new string [] {@"57g??????sE??kPdyd9R????3QNQF", @"r??UG", @"c????T??a??ou????????????v??????????c????b??????Lb??w??????j??", @"????????7??h??nO??Y??9????X", @"eIN??F????S??0o??P??????x????y????C????p", @"????yu????rx??iMi", @"0??qtN????????????CWB??P??O????B????????1y??????S????P6????GQ??????o??s??", @"HIb??????z??KA7??q5Co??wa??Aj99WbU????????6r????", @"??Dzo??????TyY????????UiV??6NF??????????????K????", @"??cb??nRtu??V??og??Jw??feA06??tjfL????74??z??m??????", @"??2FI??Ea??f??c????Kg??t", @"??n??p??", @"5??????X??mqrN??3xL??Y??M????AHk??????m????Y??jBC??r??r??G", @"Vd??vMX??d??HYx??aeW??????3k", @"B??Ph????u??d????w??rC7pR??T??9J??yOV????????Vuo??d??I??9??g", @"8??", @"??c????RC??y????????N??g??I??Q??????KH??A??B??9EWV????3????X????WPmB"}, 
			new string [] {@"j??I", @"B????f????ku??C??j7N??E2P??????41p??????", @"kf????s??5??????4L??5l??n??r5KhDuK??X??YG????", @"FN????AJG????????nt????S????aJ??td??kE??rD??p??zE??g??h7??z", @"w??????O??????????7X??I????????mh2Q??4??t??WW??m????5????????UX??T", @"??L??Z??vg52??B??2????AI??h??6T????????T??????V????????????", @"Up??D4??", @"??????S??a4", @"??c??????????01????", @"fu??????L??s??????????I????wjq??????L??I", @"????????aZu??Y??????5??????????KN??VaQ??1??S??????u0V??IVjVe3??ZV????H??T", @"Ub??P??opoHx????Aq", @"m", @"U????????h????????uYR????T??fxMJQ7??D??IJ????????xJ??v??????j??b??????QCE??", @"??AA0??tiB??Ku??????G??7w??k??yaDwA????T??u????sq4P????E"}, 
			new string [] {@"9e??R????????????w0W1??5??w??????????????w??b??x????SF", @"??IJw??f????W??yFM??jt6j8s????????u??8qc??I1??6??????O??W????G????????", @"??9??u??G??", @"??3aICoS??F??????u??J??????w??z??8", @"K??????????M????Z????????lGcDOrqL??R??6k2??P????K??????????????P", @"mBOd??5e989k??nH????z??9G??????n0", @"Iqhq??Fj5????c????I??Od????x5????q??fx????????", @"????Y7??NR??5??PM??H??v??????C??I????1G??DP", @"QsI", @"??7??yLI????jgCr??0??QT????Ivn????Do??E??qboHd8az??", @"????xJ??6Y??i??o??0????Su????9????i????Q??aaSUPEGvOx", @"??", @"Zn??3????", @"F????????yO??????????O??0MB??Ig????????A????Z????S", @"??Q????R1yq??S????????????xJ??hY??????????wa", @"6GO6????c4aD??X6jo??1????1??fuFB9????uI????h??Z????B??93", @"LhC??A??F??C??hN??B??????E????????I7??????eo????KX"}, 
			new string [] {@"iL????PO??????7??tT4??B??8????G????E??u5N??S????u9o????exrk??M????d??", @"??p??kRm????r??n??4g????????27L????????J??muu????n??H3", @"??Ih??b??m??V????????l????V????????G2??t2Q??E??K????l1dBX9cV????p??T", @"7??bD1??UmJ"}, 
			new string [] {@"??7y??9e??????xND??????o????E", @"??????BTv??IZ??J??Og3????c0????????????e????????????Xe????D????p??", @"8xufM??5IN??F????yY??n????2??????N??????????u", @"j??NTP??h??g??q??????Q??", @"??RY4??k??gk??H??XD????????z??9b??D??X??????4ZgVS5??1??s", @"LY??Y??aBEhnG????????", @"??HI????????G", @"KH??????z??G??q????6??f7??at????????bxg??oGLc", @"o??????q??lo????3Z??????fS????n4??????????Q????pwO??????2??", @"b", @"37Z????????Lfy????3??????f????????R9w????????Yq??a????JN??g", @"f??n????9??s????????O1F????zyes09h????????6", @"jr9E????Q??W??????js", @"Q??yF??Yo??ig03lKNfOgT??c????i??2??G????????????si????", @"??????WJ", @"XiULjBO????aqgw??j7??Qk", @"??za????l7??K????????b??0IN??hC????", @"??fw??xqb??Mr??a??B1??pq??????????", @"??r??xhI??kV????9o5????6????8??"}, 
			new string [] {@"????Y9??", @"BDiP??????r??6zQ????PBm", @"??Y????s??D????????uZ0N??1??S????Fi??", @"", @"IX??????vFW????????iW????????ldJ??v??4??V??????p", @"????C??XS", @"??PR????????yF????????wuwp??H??2o3lH??g????6??S??N????M3??????p??7????????", @"O??1w6N0??4????Mxt????", @"3V??????qGO??U????c??FxYme??????????8????u??FB??????A??????KWZ??V", @"rb??????????O??????fxK??????8Rs??Y????z??????y????b????????", @"d??1F??????Eo??cA??????", @"yx??????z??C????????????Sd????O????3??8????Z??P????p??ok??????p5??MK??????????R", @"??????oe1TfV5????3??ryt????????Vs??sS??w??e??FR????J??3wO??y????IeX??", @"e??????eJ??du3b??B??l??????j??g??R??23j????????I????", @"g??oq??7??", @"ou??V??FXey4??", @"r??????Q??????????xw????????J8L??ZW??????????sw??????V????", @"L??y????7????N????P??b??f??S??hcL????Ssi??????J??a??5Wn69??A31e??????"}, 
			new string [] {@"????R????Y0CX??P??????????e??????R????y", @"nrln1", @"????nO??O??nM7??t??Y1??g??I??????E??????SvCN??eFp????????D??exmPF????vc"}, 
			new string [] {@"g????hLb????????W??XvBN????A??l??4??UQh??p????", @"????7????3????Q????ZDS", @"I??1xMOlH??R????", @"????V??V??9????fCU????eHf??Duxf??6??8??4??????Arh????????????F", @"GF??l????j??Vh??????BJ1??x????1G????", @"??jn??o??Pf6??p??0", @"????N????aTmc????????gj??NI????e????F??????Y??3j????R??LH??????????????3", @"????X??????w??Bln??11vi2", @"SQ????????y??rZs??Gg??a??????????46U????????VV????I????????m??", @"??c1????????mQ??8??M??iq????d??H????????????md??m", @"????R", @"7x??u??????b????M??4L??????j????", @"??????????2??A??m????kT??rg??V??i????XE????H??????????JQ", @"??"}, 
			new string [] {@"lyQY??oR??UaqcOD????u8????Z??4L??U??r??P????T2??????K????????J??p??", @"????pn????O????Ig0QgO????L5??v8??f??FN??W"}, 
			new string [] {@"??njo??????kuwY??7??2cs????D??????G????2m", @"??g????u??7??PJ??Bu????L97??o??7??TtP????t1h????????TR??????????L????FKC??", @"????????l????l??ta??z??????A????N", @"pR??Q??Q??????L5????X2??????DPo????wwH??", @"??uST????", @"??????Cr??oTD????h????E??w??J9??LW??????n??", @"??j????c??J??8??????", @"????9??????p????AmB2VEUZ??t??Fz????????x", @"??????X????A??ZV????M??2PW??????U????f??????", @"oK????g??K??????CyC??29??7????H??i??????????LJ??6????????????bku", @"EX????I??H????XK????dWI????LR??J??????q??9????b??W????x", @"Zp????6??o??????u44t??hdsnj??eiD??y????????????0??3??A1??????", @"h????????????", @"????XR4Vd"}, 
			new string [] {@"iIZm", @"????????n??g"}, 
			new string [] {@"??ik2DQH????wa??????", @"uv????WD??m??fQ????????JB????????E??a??L????????????????", @"??????????????g????", @"7??", @"js??CkzaFAA??GDm??????????6l????8??", @"??????????ZAv??c??Ay??Ya??1ul9??DqZ??????m3??E", @"ead??jFL??G????Fo??Y????a??????D??2I??0??TS4Q????yu??z????O??pvw24??", @"????NA????QX????", @"??cGQ??C", @"3o??B7t??Cx????????6??x????L????u5j??Ng??L22??"}, 
			new string [] {}
		};
		static object [] state_methods_values = new object []  {
			/*new object [] {
				"here goes name of method", new object [] {"arg1", 2, "etc", "must match type exactly"}
				},*/
			new object [] {
				new object [] {"Add", new object [] {"a"}},
				new object [] {"Add", new object [] {'a'}},
				new object [] {"Add", new object [] {'a', 1, MS_System_ComponentModel.MaskedTextResultHint.Unknown}}
				},
			new object [] {
				new object [] {"Add", new object [] {"a"}},
				new object [] {"Remove", new object [] {}},
				new object [] {"InsertAt", new object [] {'a', 1}}
				},
			new object [] {
				new object [] {"Add", new object [] {"1"}},
				new object [] {"Add", new object [] {"2"}},
				new object [] {"InsertAt", new object [] {'3', 7}},
				new object [] {"InsertAt", new object [] {'4', 4}}
				},
			new object [] {
				new object [] {"InsertAt", new object [] {'z', 0}},
				new object [] {"InsertAt", new object [] {'z', 1}},
				new object [] {"InsertAt", new object [] {'z', 2}},
				},
			new object [] {
				new object [] {"InsertAt", new object [] {'z', 0}},
				new object [] {"InsertAt", new object [] {'z', 2}},
				}
			};
		/*static string state_methods = new string [] {
			"Add",		// char, string, char+(out)int+(out)hint
			"Clear",	// -, hint
			"InsertAt",	// char+int, string+int, char+int+int+hint, string+int+int+hint
			"Remove",	// -, int+hint
			"RemoveAt",	// int, int+int, int+int+int+hint
			"Replace",	// char+int, string+int, char+int+int+hint, string+int+int+hint, char+int+int+int+hint, string+int+int+int+hint
			"Set"		// string, string+int+hint
			};
		*/
		//static public void char_gen ()
		//{
		//        string result = "";
		//        Random rnd = new Random ();
		//        result += "static char [][] add_char_test_values2 = new char [][] {" + Environment.NewLine;
		//        for (int i = 0; i < 25; i++) {
		//                result += "\tnew char [] {";
		//                int b = rnd.Next (0, 20);
		//                for (int j = 0; j < b; j++) {
		//                        int a = rnd.Next (ushort.MinValue, ushort.MaxValue + 1);
		//                        char c = Convert.ToChar (a);
		//                        result += GetStringValue (c);
		//                        if (j < b - 1)
		//                                result += ", ";
		//                        else
		//                                result += "}";
		//                }
		//                if (i < 24)
		//                        result += ", ";
		//                else
		//                        result += Environment.NewLine + "}";
		//                result += Environment.NewLine;
		//        }
		//        MS_System.System.Diagnostics.Debug.WriteLine (result);
		//}
		//static public void str_gen ()
		//{
		//        string result = "";
		//        Random rnd = new Random ();
		//        result += "static string [][] add_string_test_values2 = new string [][] {" + Environment.NewLine;
		//        for (int i = 0; i < 25; i++) {
		//                result += "\tnew string [] {";
		//                int b = rnd.Next (0, 20);
		//                for (int j = 0; j < b; j++) {
		//                        int c = rnd.Next (0, 50);
		//                        string str = "";
		//                        for (int k = 0; k < c; k++) {
		//                                int a;
		//                                if (i < 2) {
		//                                        a = rnd.Next (ushort.MinValue, ushort.MaxValue + 1);
		//                                } else if (i < 4) {
		//                                        do {
		//                                                a = rnd.Next (ushort.MinValue, 256);
		//                                        } while (!MS_System_ComponentModel.MaskedTextProvider.IsValidInputChar (Convert.ToChar (a)));
		//                                } else {
		//                                        do {
		//                                                a = rnd.Next (ushort.MinValue, 256);
		//                                        } while (!char.IsLetterOrDigit (Convert.ToChar (a)));
		//                                }
		//                                str += Convert.ToChar (a).ToString ();
		//                        }
		//                        result += "@\"" + str.Replace ("\"", "\"\"") + "\"";
		//                        if (j < b - 1)
		//                                result += ", ";
		//                        else
		//                                result += "}";
		//                }
		//                if (b == 0)
		//                        result += "}";
		//                if (i < 24)
		//                        result += ", ";
		//                else
		//                        result += Environment.NewLine + "};";
		//                result += Environment.NewLine;
		//        }
		//        MS_System.System.Diagnostics.Debug.WriteLine (result);
		//}

		static int Test ()
		{
			MaskedTextProviderTest tests = new MaskedTextProviderTest ();
			tests.Replace_string_int_int_int_MaskedTextResultHintTest00137 ();
			
			return 0;
		}

		static int Main (string [] args)
		{
			//return Test ();
			
			if (typeof (int).GetType ().Name != "RuntimeType") {
				Console.WriteLine ("This must be run on the MS runtime.");
				return 1;
			}

			string file = ""; 
			// Check that this path is correct before removing the comment.
			// file = "..\\..\\Test\\System.ComponentModel\\MaskedTextProviderTestGenerated.cs";
			// file = @"Z:\mono\head\mcs\class\System\Test\System.ComponentModel\MaskedTextProviderTestGenerated.cs";
			
			destination_file = file;
			
			if (destination_file == "") {
				Console.WriteLine ("You'll have to set the destination file. See source for instructions.");
				return 1;
			}

			using (StreamWriter stream = new StreamWriter (new FileStream (file, FileMode.Create, FileAccess.Write), Encoding.Unicode)) {
				writer = stream;
				WriteFileHeader ();
			
				GenerateAdd_char_int_MaskedTextResultHint_Test ();
				GenerateAdd_char_Test ();
				GenerateAdd_string_int_MaskedTextResultHint_Test ();
				GenerateAdd_string_Test ();
				GenerateClear_MaskedTextResultHint_Test ();
				GenerateClearTest ();
				GenerateCloneTest ();
				GenerateEditPositionsTest ();
				GenerateFindAssignedEditPositionFromTest ();
				GenerateFindAssignedEditPositionInRangeTest ();
				GenerateFindEditPositionFromTest ();
				GenerateFindEditPositionInRangeTest ();
				GenerateFindNonEditPositionFromTest ();
				GenerateFindNonEditPositionInRangeTest ();
				GenerateFindUnassignedEditPositionFromTest ();
				GenerateFindUnassignedEditPositionInRangeTest ();
				GenerateInsertAt_char_int_Test ();
				GenerateInsertAt_char_int_int_MaskedTextResultHintTest ();
				GenerateInsertAt_string_int_int_MaskedTextResultHintTest ();
				GenerateInsertAt_string_int_Test ();
				GenerateIsAvailablePositionTest ();
				GenerateIsEditPositionTest ();
				GenerateIsValidInputCharTest ();
				GenerateIsValidMaskCharTest ();
				GenerateIsValidPasswordCharTest ();
				GenerateItemTest ();
				GenerateRemoveTest ();
				GenerateRemove_int_MaskedTextResultHintTest ();
				GenerateRemoveAt_int_int_int_MaskedTextResultHintTest ();
				GenerateRemoveAt_int_int_Test ();
				GenerateRemoveAt_int_Test ();
				GenerateReplace_char_int_int_int_MaskedTextResultHintTest ();
				GenerateReplace_char_int_int_MaskedTextResultHintTest ();
				GenerateReplace_char_int_Test ();
				GenerateReplace_string_int_int_int_MaskedTextResultHintTest ();
				GenerateReplace_string_int_int_MaskedTextResultHintTest ();
				GenerateReplace_string_int_Test ();
				GenerateSet_string_int_MaskedTextResultHintTest ();
				GenerateSet_string_Test ();
				GenerateToDisplayStringTest ();
				GenerateToString_bool_bool_bool_int_int_Test ();
				GenerateToString_bool_bool_int_int_Test ();
				GenerateToString_bool_bool_Test ();
				GenerateToString_bool_int_int_Test ();
				GenerateToString_bool_Test ();
				GenerateToString_int_int_Test ();
				GenerateToStringTest ();
				GenerateVerifyCharTest ();
				GenerateVerifyEscapeCharTest ();
				GenerateVerifyString_string_int_MaskedTextResultHintTest ();
				GenerateVerifyString_string_Test ();

				WriteFileFooter ();
			}
			
			Console.WriteLine ("Press any key to exit.");
			Console.Read ();
			return 0;
		}

		static void GenerateAdd_char_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			bool result;
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");

			foreach (string mask in test_masks) {
				foreach (char [] chars in add_char_test_values) {
					foreach (char c in chars) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { c };
							if (Compare ("Add", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							result = mtp.Add (c);
							WriteLine (string.Format ("result = mtp.Add ('\\x{0:X4}');", (int)c) + (c != char.MinValue ? "/* " + c.ToString () + " */" : "/* null */"));
							WriteLine ("Assert.AreEqual ({0}, result, \"{1}#{2}\");", GetStringValue (result), TestName, (counter++).ToString ());
							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}
			WriteTestFooter ();
		}
		static void GenerateAdd_char_int_MaskedTextResultHint_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");

			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int testPosition = 0;
			MS_System_ComponentModel.MaskedTextResultHint resultHint = MS_System_ComponentModel.MaskedTextResultHint.Unknown;
			bool result;

			foreach (string mask in test_masks) {
				foreach (char [] chars in add_char_test_values) {
					foreach (char c in chars) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { c, testPosition, resultHint };
							if (Compare ("Add", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");

							more_states = CreateState (mtp, stateindex);
							stateindex++;

							result = mtp.Add (c, out testPosition, out resultHint);
							WriteLine (string.Format ("result = mtp.Add ('\\x{0:X4}', out testPosition, out resultHint);", (int)c) + (c != char.MinValue ? "/* " + c.ToString () + " */" : "/* null */"));
							WriteLine ("Assert.AreEqual ({0}, result, \"{1}#{2}\");", GetStringValue (result), TestName, (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, resultHint, \"{1}#{2}\");", GetStringValue (resultHint), TestName, (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, testPosition, \"{1}#{2}\");", GetStringValue (testPosition), TestName, (counter++).ToString ());
							WriteAssertProperties (mtp, Name, TestName, ref counter);

							WriteTestEnd ();
						} while (more_states);
					}
				}
			}
			WriteTestFooter ();
		}
		static void GenerateAdd_string_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");

			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			bool result;

			foreach (string mask in test_masks) {
				foreach (string [] strings in add_string_test_values) {
					foreach (string s in strings) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { s };
							if (Compare ("Add", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;
							result = mtp.Add (s);
							WriteLineNonFormat ("result = mtp.Add (@\"" + s.Replace ("\"", "\"\"") + "\");");
							WriteLine ("Assert.AreEqual ({0}, result, \"{1}#{2}\");", GetStringValue (result), TestName, (counter++).ToString ());
							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}
			WriteTestFooter ();
		}
		static void GenerateAdd_string_int_MaskedTextResultHint_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");

			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int testPosition = 0;
			MS_System_ComponentModel.MaskedTextResultHint resultHint = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			bool result;

			foreach (string mask in test_masks) {
				foreach (string [] strings in add_string_test_values) {
					foreach (string s in strings) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { s, testPosition, resultHint };
							if (Compare ("Add", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}
							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							result = mtp.Add (s, out testPosition, out resultHint);
							WriteLineNonFormat ("result = mtp.Add (@\"" + s.Replace ("\"", "\"\"") + "\", out testPosition, out resultHint);");
							WriteLine ("Assert.AreEqual ({0}, result, \"{1}#{2}\");", GetStringValue (result), TestName, (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, resultHint, \"{1}#{2}\");", GetStringValue (resultHint), TestName, (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, testPosition, \"{1}#{2}\");", GetStringValue (testPosition), TestName, (counter++).ToString ());
							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteLine ("");
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}
			WriteTestFooter ();
		}

		static void GenerateClearTest ()
		{
			GenerateClear_MaskedTextResultHint_Test (false);
		}
		static void GenerateClear_MaskedTextResultHint_Test ()
		{
			GenerateClear_MaskedTextResultHint_Test (true);
		}

		static void GenerateClear_MaskedTextResultHint_Test (bool with_result)
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			if (!with_result) {
				TestName = TestName.Replace ("_MaskedTextResultHint_", "");
			}

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");

			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			MS_System_ComponentModel.MaskedTextResultHint resultHint = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				int stateindex = 0;
				do {

					object [] arguments;
					arguments = new object [] { resultHint };
					if (Compare ("Clear", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}

					WriteTestStart ();
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
					more_states = CreateState (mtp, stateindex);
					stateindex++;

					if (with_result) {
						mtp.Clear (out resultHint);
						WriteLine ("mtp.Clear (out resultHint);");
						WriteLine ("Assert.AreEqual ({0}, resultHint, \"{1}#{2}\");", GetStringValue (resultHint), TestName, (counter++).ToString ());
					} else {
						mtp.Clear ();
						WriteLine ("mtp.Clear ();");
					}
					WriteAssertProperties (mtp, Name, TestName, ref counter);
					WriteTestEnd ();
				} while (more_states);
			}
			WriteTestFooter ();
		}
		static void GenerateCloneTest ()
		{
			////string Name = "mtp";
			//string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			////int counter = 0;


			//WriteTestHeader (TestName);
			//WriteLine ("");
			//WriteTestStart ();
			//WriteLine ("Assert.Ignore (\"Only manual tests here for the moment.\");");
			//WriteTestEnd ();
			//WriteTestFooter ();
		}

		static void GenerateEditPositionsTest ()
		{
			int counter = 0;
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			string TestName = "EditPositionsTestGenerated";

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int testPosition;", "MaskedTextResultHint resultHint;", "bool result;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			WriteLine ("");
			foreach (string mask in test_masks) {
				int stateindex = 0;
				bool more_states = true;
				do {
					object [] arguments;
					arguments = new object [] {};
					if (Compare ("EditPositions", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}
					
					WriteTestStart ();

					string new_statement = "mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");";
					more_states = CreateState (mtp, stateindex);
					stateindex++;
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					string result = MaskedTextProviderTest.join (mtp.EditPositions, ";");
					WriteLine ("");
					WriteLine (new_statement);
					WriteLine ("Assert.AreEqual (\"" + result + "\", MaskedTextProviderTest.join (mtp.EditPositions, \";\"), \"{0}#{1}\");", TestName, (counter++).ToString ());
					WriteTestEnd ();
				} while (more_states);
			}

			WriteTestFooter ();

		}

		static void GenerateFindFromTest (string methodName)
		{
			string Name = "mtp";
			string TestName = methodName + "Test";
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			foreach (string mask in test_masks) {
				foreach (bool value in new bool [] { true, false }) {
					for (int i = 0; i < mask.Length + 2; i++) {
						int stateindex = 0;
						bool more_states = true;
						do {
							object [] arguments;
							arguments = new object [] { i, value };
							if (Compare (methodName, mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp." + methodName + "({1}, {3}), \"#{2}\");", mtp.GetType ().InvokeMember (methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mtp.GetType (), arguments).ToString (), i.ToString (), (counter++).ToString (), value ? "true" : "false");

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}
			WriteTestFooter ();
		}
		static void GenerateFindRangeTest (string methodName)
		{
			string Name = "mtp";
			string TestName = methodName + "Test";
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			foreach (string mask in test_masks) {
				foreach (bool value in new bool [] { true, false }) {
					for (int i = 0; i < mask.Length + 2; i++) {
						for (int k = 0; k < mask.Length + 2; k++) {

							int stateindex = 0;
							bool more_states = true;
							do {

								object [] arguments;
								arguments = new object [] { i, k, value };
								if (Compare (methodName, mask, ref stateindex, arguments, ref more_states)) {
									continue;
								}

								WriteTestStart ();
								mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
								WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
								more_states = CreateState (mtp, stateindex);
								stateindex++;

								WriteLine ("Assert.AreEqual ({0}, mtp." + methodName + " ({1}, {2}, {4}), \"#{3}\");", mtp.GetType ().InvokeMember (methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mtp.GetType (), arguments).ToString (), i.ToString (), k.ToString (), (counter++).ToString (), value ? "true" : "false");

								WriteAssertProperties (mtp, Name, TestName, ref counter);
								WriteTestEnd ();

							} while (more_states);
						}
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateFindAssignedEditPositionFromTest ()
		{
			GenerateFindFromTest ("FindAssignedEditPositionFrom");
		}
		static void GenerateFindAssignedEditPositionInRangeTest ()
		{
			GenerateFindRangeTest ("FindAssignedEditPositionInRange");
		}
		static void GenerateFindEditPositionFromTest ()
		{
			GenerateFindFromTest ("FindEditPositionFrom");
		}
		static void GenerateFindEditPositionInRangeTest ()
		{
			GenerateFindRangeTest ("FindEditPositionInRange");
		}
		static void GenerateFindNonEditPositionFromTest ()
		{
			GenerateFindFromTest ("FindNonEditPositionFrom");
		}
		static void GenerateFindNonEditPositionInRangeTest ()
		{
			GenerateFindRangeTest ("FindNonEditPositionInRange");
		}
		static void GenerateFindUnassignedEditPositionFromTest ()
		{
			GenerateFindFromTest ("FindUnassignedEditPositionFrom");
		}
		static void GenerateFindUnassignedEditPositionInRangeTest ()
		{
			GenerateFindRangeTest ("FindUnassignedEditPositionInRange");
		}

		static void GenerateInsertAt_char_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			foreach (string mask in test_masks) {
				foreach (char chr in char_values) {
					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { chr, i };
							if (Compare ("InsertAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.InsertAt ({1}, {2}), \"#{3}\");", GetStringValue (mtp.InsertAt (chr, i)), GetStringValue (chr), i.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateInsertAt_string_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { str, i };
							if (Compare ("InsertAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.InsertAt ({1}, {2}), \"#{3}\");", GetStringValue (mtp.InsertAt (str, i)), GetStringValue (str), i.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateInsertAt_char_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char chr in char_values) {
					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { chr, i, Int32_out, MaskedTextResultHint_out };
							if (Compare ("InsertAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;
							WriteLine ("Assert.AreEqual ({0}, mtp.InsertAt ({1}, {2}, out Int32_out, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.InsertAt (chr, i, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (chr), i.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateInsertAt_string_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {

				foreach (string str in string_values) {
					if (str == null)
						continue;

					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {

							object [] arguments;
							arguments = new object [] { str, i, Int32_out, MaskedTextResultHint_out };
							if (Compare ("InsertAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.InsertAt ({1}, {2}, out Int32_out, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.InsertAt (str, i, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateIsAvailablePositionTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = -1; i < mask.Length + 2; i++) {
					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { i };
						if (Compare ("IsAvailablePosition", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.IsAvailablePosition ({1}), \"#{2}\");", GetStringValue (mtp.IsAvailablePosition (i)), i.ToString (), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);
				}
			}

			WriteTestFooter ();
		}
		static void GenerateIsEditPositionTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;


			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = -1; i < mask.Length + 2; i++) {
					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { i };
						if (Compare ("IsEditPosition", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.IsEditPosition ({1}), \"#{2}\");", GetStringValue (mtp.IsEditPosition (i)), i.ToString (), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);
				}
			}

			WriteTestFooter ();
		}
		static void GenerateIsValidInputCharTest ()
		{
			string TestName = "IsValidInputCharTestGenerated";

			dont_write = true;
doagain:
			WriteTestHeader (TestName);
			WriteTestStart ();

			int max = (int)char.MaxValue;
			BitArray bits = new BitArray (max);
			for (int i = 0; i < max; i++) {
				bool result_MS = MS_System_ComponentModel.MaskedTextProvider.IsValidInputChar ((char)i);
				bool result_Mono = MaskedTextProvider.IsValidInputChar ((char) i);
				if (dont_write && result_MS != result_Mono) {
					dont_write = false;
					goto doagain;
				}
				bits.Set (i, result_MS);
				
			}
			StringBuilder bit_array = new StringBuilder ();
			bit_array.AppendLine ("int [] answers;");
			bit_array.AppendLine (tabs + "unchecked {");
			bit_array.Append (tabs + "answers = new int [] {");
			int [] numbers = new int [max / 32 + 1];
			bits.CopyTo (numbers, 0);
			for (int i = 0; i < numbers.Length; i++) {
				if (i % 8 == 0) {
					bit_array.AppendLine ("");
					bit_array.Append (tabs + "\t");
				}
				bit_array.Append (String.Format ("(int) 0x{0:X8}", numbers [i]));
				if (i != numbers.Length - 1)
					bit_array.Append (", ");

			}
			bit_array.AppendLine ("");
			bit_array.AppendLine (tabs + "};");
			bit_array.AppendLine (tabs + "};");
			WriteLineNonFormat (bit_array.ToString ());
			WriteLine ("BitArray bits = new BitArray (answers);");
			WriteLine ("for (int i = 0; i < (int) char.MaxValue; i++)");
			WriteLine ("\tAssert.AreEqual (bits.Get (i), MaskedTextProvider.IsValidInputChar ((char) i), \"{0}#\" + i.ToString ());", TestName);

			WriteTestEnd ();
			WriteTestFooter ();
		}
		static void GenerateIsValidMaskCharTest ()
		{
			string TestName = "IsValidMaskCharTestGenerated";

			dont_write = true;
		doagain:
		
			WriteTestHeader (TestName);
			WriteTestStart ();

			int max = (int)char.MaxValue;
			BitArray bits = new BitArray (max);
			for (int i = 0; i < max; i++) {
				bool result_MS = MS_System_ComponentModel.MaskedTextProvider.IsValidMaskChar ((char)i);
				bool result_Mono = MaskedTextProvider.IsValidMaskChar ((char)i);
				if (dont_write && result_MS != result_Mono) {
					dont_write = false;
					goto doagain;
				}
				bits.Set (i, result_MS);
			}
			StringBuilder bit_array = new StringBuilder ();
			bit_array.AppendLine ("int [] answers;");
			bit_array.AppendLine (tabs + "unchecked {");
			bit_array.Append (tabs + "answers = new int [] {");
			int [] numbers = new int [max / 32 + 1];
			bits.CopyTo (numbers, 0);
			for (int i = 0; i < numbers.Length; i++) {
				if (i % 8 == 0) {
					bit_array.AppendLine ("");
					bit_array.Append (tabs + "\t");
				}
				bit_array.Append (String.Format ("(int) 0x{0:X8}", numbers [i]));
				if (i != numbers.Length - 1)
					bit_array.Append (", ");

			}
			bit_array.AppendLine ("");
			bit_array.AppendLine (tabs + "};");
			bit_array.AppendLine (tabs + "};");
			WriteLineNonFormat (bit_array.ToString ());
			WriteLine ("BitArray bits = new BitArray (answers);");
			WriteLine ("for (int i = 0; i < (int) char.MaxValue; i++)");
			WriteLine ("\tAssert.AreEqual (bits.Get (i), MaskedTextProvider.IsValidMaskChar ((char) i), \"{0}#\" + i.ToString ());", TestName);

			WriteTestEnd ();
			WriteTestFooter ();
		}
		static void GenerateIsValidPasswordCharTest ()
		{
			string TestName = "IsValidPasswordCharGenerated";

			dont_write = true;
		doagain:
		
			WriteTestHeader (TestName);
			WriteTestStart ();


			int max = (int)char.MaxValue;
			BitArray bits = new BitArray (max);
			for (int i = 0; i < max; i++) {
				bool result_MS = MS_System_ComponentModel.MaskedTextProvider.IsValidPasswordChar ((char)i);
				bool result_Mono = MaskedTextProvider.IsValidPasswordChar ((char)i);
				if (dont_write && (result_MS != result_Mono)) {
					dont_write = false;
					goto doagain;
				}
				bits.Set (i, MS_System_ComponentModel.MaskedTextProvider.IsValidPasswordChar ((char)i));
			}
			StringBuilder bit_array = new StringBuilder ();
			bit_array.AppendLine ("int [] answers;");
			bit_array.AppendLine (tabs + "unchecked {");
			bit_array.Append (tabs + "answers = new int [] {");
			int [] numbers = new int [max / 32 + 1];
			bits.CopyTo (numbers, 0);
			for (int i = 0; i < numbers.Length; i++) {
				if (i % 8 == 0) {
					bit_array.AppendLine ("");
					bit_array.Append (tabs + "\t");
				}
				bit_array.Append (String.Format ("(int) 0x{0:X8}", numbers [i]));
				if (i != numbers.Length - 1)
					bit_array.Append (", ");

			}
			bit_array.AppendLine ("");
			bit_array.AppendLine (tabs + "};");
			bit_array.AppendLine (tabs + "};");
			WriteLineNonFormat (bit_array.ToString ());
			WriteLine ("BitArray bits = new BitArray (answers);");
			WriteLine ("for (int i = 0; i < (int) char.MaxValue; i++)");
			WriteLine ("\tAssert.AreEqual (bits.Get (i), MaskedTextProvider.IsValidPasswordChar ((char) i), \"{0}#\" + i.ToString ());", TestName);

			WriteTestEnd ();
			WriteTestFooter ();
		}

		static void GenerateItemTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;

			foreach (string mask in test_masks) {
				for (int i = 0; i < mask.Length; i++) {
					bool more_states = true;
					int stateindex = 0;
					do {
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);

						if (i >= mtp.Length)
							break;

						object [] arguments;
						arguments = new object [] { i };
						if (Compare ("Item", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp [{1}], \"#{2}\");", GetStringValue (mtp [i]), i.ToString (), (counter++).ToString ());
						WriteAssertProperties (mtp, Name, TestName, ref counter);

						WriteTestEnd ();

					} while (more_states);
				}
			}

			WriteTestFooter ();

		}

		static void GenerateRemoveTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				int stateindex = 0;
				do {

					object [] arguments;
					arguments = new object [] { };
					if (Compare ("Remove", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}


					WriteTestStart ();
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
					more_states = CreateState (mtp, stateindex);
					stateindex++;

					for (int i = -1; i < mask.Length + 2; i++) {
						WriteLine ("Assert.AreEqual ({0}, mtp.Remove (), \"#{1}\");", GetStringValue (mtp.Remove ()), (counter++).ToString ());
						WriteAssertProperties (mtp, Name, TestName, ref counter);
					}
					WriteTestEnd ();

				} while (more_states);
			}

			WriteTestFooter ();
		}
		static void GenerateRemove_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				int stateindex = 0;
				do {

					object [] arguments;
					arguments = new object [] { Int32_out, MaskedTextResultHint_out };
					if (Compare ("Remove", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}

					WriteTestStart ();
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
					more_states = CreateState (mtp, stateindex);
					stateindex++;

					for (int i = -1; i < mask.Length + 2; i++) {
						WriteLine ("Assert.AreEqual ({0}, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), \"#{1}\");", GetStringValue (mtp.Remove (out Int32_out, out MaskedTextResultHint_out)), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());
						WriteAssertProperties (mtp, Name, TestName, ref counter);
					}
					WriteTestEnd ();
				} while (more_states);
			}

			WriteTestFooter ();
		}
		static void GenerateRemoveAt_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;


			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = 0; i < mask.Length; i++) {
					bool more_states = true;
					int stateindex = 0;
					do {

						object [] arguments;
						arguments = new object [] { i };
						if (Compare ("RemoveAt", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.RemoveAt ({1}), \"#{2}\");",
							GetStringValue (mtp.RemoveAt (i)), i.ToString (), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);
				}
			}

			WriteTestFooter ();
		}
		static void GenerateRemoveAt_int_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = 0; i < mask.Length; i++) {
					for (int j = 0; j < mask.Length; j++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { i, j };
							if (Compare ("RemoveAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.RemoveAt ({1}, {2}), \"#{3}\");",
								GetStringValue (mtp.RemoveAt (i, j)), i.ToString (), j.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateRemoveAt_int_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//object mtp_MS, mtp_Mono;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = 0; i < mask.Length; i++) {
					for (int j = 0; j < mask.Length; j++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { i, j, Int32_out, MaskedTextResultHint_out };
							if (Compare ("RemoveAt", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							dont_write = false;
							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.RemoveAt ({1}, {2}, out Int32_out, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.RemoveAt (i, j, out Int32_out, out MaskedTextResultHint_out)), i.ToString (), j.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}


			WriteTestFooter ();
		}

		static void GenerateReplace_char_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char str in char_values) {
					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { str, i };
							if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}), \"#{3}\");",
								GetStringValue (mtp.Replace (str, i)), GetStringValue (str), i.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateReplace_string_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { str, i };
							if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}), \"#{3}\");",
								GetStringValue (mtp.Replace (str, i)), GetStringValue (str), i.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateReplace_char_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char str in char_values) {
					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { str, i, Int32_out, MaskedTextResultHint_out };
							if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}, out Int32_out, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.Replace (str, i, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateReplace_string_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;


			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					for (int i = 0; i < mask.Length; i++) {
						bool more_states = true;
						int stateindex = 0;

						do {
							object [] arguments;
							arguments = new object [] { str, i, Int32_out, MaskedTextResultHint_out };
							if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}, out Int32_out, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.Replace (str, i, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateReplace_char_int_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char str in char_values) {
					for (int i = 0; i < mask.Length; i++) {
						for (int j = 0; j < mask.Length; j++) {
							bool more_states = true;
							int stateindex = 0;

							do {
								object [] arguments;
								arguments = new object [] { str, i, j, Int32_out, MaskedTextResultHint_out };
								if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
									continue;
								}

								WriteTestStart ();
								mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
								WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
								more_states = CreateState (mtp, stateindex);
								stateindex++;

								WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}, {3}, out Int32_out, out MaskedTextResultHint_out), \"#{4}\");",
									GetStringValue (mtp.Replace (str, i, j, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), j.ToString (), (counter++).ToString ());
								WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
								WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

								WriteAssertProperties (mtp, Name, TestName, ref counter);
								WriteTestEnd ();

							} while (more_states);
						}
					}
				}
			}
			WriteTestFooter ();
		}
		static void GenerateReplace_string_int_int_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					for (int i = 0; i < mask.Length; i++) {
						for (int j = 0; j < mask.Length; j++) {
							bool more_states = true;
							int stateindex = 0;

							do {
								object [] arguments;
								arguments = new object [] { str, i, j, Int32_out, MaskedTextResultHint_out };
								if (Compare ("Replace", mask, ref stateindex, arguments, ref more_states)) {
									continue;
								}

								WriteTestStart ();
								mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
								WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
								more_states = CreateState (mtp, stateindex);
								stateindex++;

								WriteLine ("Assert.AreEqual ({0}, mtp.Replace ({1}, {2}, {3}, out Int32_out, out MaskedTextResultHint_out), \"#{4}\");",
									GetStringValue (mtp.Replace (str, i, j, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), j.ToString (), (counter++).ToString ());
								WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
								WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

								WriteAssertProperties (mtp, Name, TestName, ref counter);
								WriteTestEnd ();

							} while (more_states && current_test_counter < MAXFAILEDTESTS);
						}
					}
				}
			}

			WriteTestFooter ();
		}

		static void GenerateSet_string_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { str };
						if (Compare ("Set", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.Set ({1}), \"#{2}\");",
							GetStringValue (mtp.Set (str)), GetStringValue (str), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);

				}
			}

			WriteTestFooter ();
		}
		static void GenerateSet_string_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					if (str == null)
						continue;

					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { str, Int32_out, MaskedTextResultHint_out };
						if (Compare ("Set", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.Set ({1}, out Int32_out, out MaskedTextResultHint_out), \"#{2}\");",
							GetStringValue (mtp.Set (str, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);

				}
			}

			WriteTestFooter ();
		}

		static void GenerateToDisplayStringTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				int stateindex = 0;
				do {
					object [] arguments;
					arguments = new object [] { };
					if (Compare ("ToDisplayString", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}

					WriteTestStart ();
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
					more_states = CreateState (mtp, stateindex);
					stateindex++;

					WriteLine ("Assert.AreEqual ({0}, mtp.ToDisplayString (), \"#{1}\");",
						GetStringValue (mtp.ToDisplayString ()), (counter++).ToString ());

					WriteAssertProperties (mtp, Name, TestName, ref counter);
					WriteTestEnd ();

				} while (more_states);
			}

			WriteTestFooter ();
		}
		static void GenerateToStringTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				int stateindex = 0;
				do {
					object [] arguments;
					arguments = new object [] { };
					if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
						continue;
					}

					WriteTestStart ();
					mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
					WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
					more_states = CreateState (mtp, stateindex);
					stateindex++;

					WriteLine ("Assert.AreEqual ({0}, mtp.ToString (), \"#{1}\");",
						GetStringValue (mtp.ToString ()), (counter++).ToString ());

					WriteAssertProperties (mtp, Name, TestName, ref counter);
					WriteTestEnd ();

				} while (more_states);
			}
			WriteTestFooter ();
		}
		static void GenerateToString_bool_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				bool more_states = true;
				foreach (bool value in new bool [] { true, false }) {
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { value };
						if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({2}), \"#{1}\");",
							GetStringValue (mtp.ToString (value)), (counter++).ToString (), value ? "true" : "false");

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();
					} while (more_states);
				}
			}

			WriteTestFooter ();
		}
		static void GenerateToString_bool_bool_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (bool value1 in new bool [] { true, false }) {
					foreach (bool value2 in new bool [] { true, false }) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { value1, value2 };
							if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({2}, {3}), \"#{1}\");",
								GetStringValue (mtp.ToString (value1, value2)), (counter++).ToString (), value1 ? "true" : "false", value2 ? "true" : "false");

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateToString_int_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				for (int i = -1; i < mask.Length + 1; i++) {
					for (int j = -1; j < mask.Length + 1; j++) {

						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { i, j };
							if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({2}, {3}), \"#{1}\");",
								GetStringValue (mtp.ToString (i, j)), (counter++).ToString (), i.ToString (), j.ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();
						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateToString_bool_int_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (bool value1 in new bool [] { true, false }) {
					for (int i = -1; i < mask.Length + 1; i++) {
						for (int j = -1; j < mask.Length + 1; j++) {

							bool more_states = true;
							int stateindex = 0;
							do {
								object [] arguments;
								arguments = new object [] { value1, i, j };
								if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
									continue;
								}

								WriteTestStart ();
								mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
								WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
								more_states = CreateState (mtp, stateindex);
								stateindex++;

								WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({4}, {2}, {3}), \"#{1}\");",
									GetStringValue (mtp.ToString (value1, i, j)), (counter++).ToString (), i.ToString (), j.ToString (), value1 ? "true" : "false");

								WriteAssertProperties (mtp, Name, TestName, ref counter);
								WriteTestEnd ();

							} while (more_states);
						}
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateToString_bool_bool_int_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (bool value1 in new bool [] { true, false }) {
					foreach (bool value2 in new bool [] { true, false }) {
						for (int i = -1; i < mask.Length + 1; i++) {
							for (int j = -1; j < mask.Length + 1; j++) {

								bool more_states = true;
								int stateindex = 0;
								do {
									object [] arguments;
									arguments = new object [] { value1, value2, i, j };
									if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
										continue;
									}

									WriteTestStart ();
									mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
									WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
									more_states = CreateState (mtp, stateindex);
									stateindex++;

									WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({4}, {5}, {2}, {3}), \"#{1}\");",
										GetStringValue (mtp.ToString (value1, value2, i, j)), (counter++).ToString (), i.ToString (), j.ToString (), value1 ? "true" : "false", value2 ? "true" : "false");

									WriteAssertProperties (mtp, Name, TestName, ref counter);
									WriteTestEnd ();

								} while (more_states);
							}
						}
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateToString_bool_bool_bool_int_int_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (bool value1 in new bool [] { true, false }) {
					foreach (bool value2 in new bool [] { true, false }) {
						foreach (bool value3 in new bool [] { true, false }) {
							for (int i = -1; i < mask.Length + 1; i++) {
								for (int j = -1; j < mask.Length + 1; j++) {

									bool more_states = true;
									int stateindex = 0;
									do {
										object [] arguments;
										arguments = new object [] { value1, value2, value3, i, j };
										if (Compare ("ToString", mask, ref stateindex, arguments, ref more_states)) {
											continue;
										}

										WriteTestStart ();
										mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
										WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
										more_states = CreateState (mtp, stateindex);
										stateindex++;

										WriteLine ("Assert.AreEqual ({0}, mtp.ToString ({4}, {5}, {6}, {2}, {3}), \"#{1}\");",
											GetStringValue (mtp.ToString (value1, value2, value3, i, j)), (counter++).ToString (), i.ToString (), j.ToString (), value1 ? "true" : "false", value2 ? "true" : "false", value3 ? "true" : "false");

										WriteAssertProperties (mtp, Name, TestName, ref counter);
										WriteTestEnd ();

									} while (more_states);
								}
							}
						}
					}
				}
			}

			WriteTestFooter ();
		}

		static void GenerateVerifyCharTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char str in char_values) {
					for (int i = -1; i < mask.Length + 1; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { str, i, MaskedTextResultHint_out };
							if (Compare ("VerifyChar", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.VerifyChar ({1}, {2}, out MaskedTextResultHint_out), \"#{3}\");",
								GetStringValue (mtp.VerifyChar (str, i, out MaskedTextResultHint_out)), GetStringValue (str), i.ToString (), (counter++).ToString ());
							WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateVerifyEscapeCharTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (char str in char_values) {
					for (int i = -1; i < mask.Length + 1; i++) {
						bool more_states = true;
						int stateindex = 0;
						do {
							object [] arguments;
							arguments = new object [] { str, i };
							if (Compare ("VerifyEscapeChar", mask, ref stateindex, arguments, ref more_states)) {
								continue;
							}

							WriteTestStart ();
							mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
							WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
							more_states = CreateState (mtp, stateindex);
							stateindex++;

							WriteLine ("Assert.AreEqual ({0}, mtp.VerifyEscapeChar ({1}, {2}), \"#{3}\");",
								GetStringValue (mtp.VerifyEscapeChar (str, i)), GetStringValue (str), i.ToString (), (counter++).ToString ());

							WriteAssertProperties (mtp, Name, TestName, ref counter);
							WriteTestEnd ();

						} while (more_states);
					}
				}
			}

			WriteTestFooter ();
		}
		static void GenerateVerifyString_string_Test ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			//int Int32_out = 0;
			//MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { str };
						if (Compare ("VerifyString", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.VerifyString ({1}), \"#{2}\");",
							GetStringValue (mtp.VerifyString (str)), GetStringValue (str), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();

					} while (more_states);
				}
			}

			WriteTestFooter ();
		}
		static void GenerateVerifyString_string_int_MaskedTextResultHintTest ()
		{
			string Name = "mtp";
			string TestName = MethodInfo.GetCurrentMethod ().Name.Replace ("Generate", "");
			int counter = 0;

			WriteTestHeader (TestName, "MaskedTextProvider mtp;", "int Int32_out = 0;", "MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;");
			MS_System_ComponentModel.MaskedTextProvider mtp = null;
			int Int32_out = 0;
			MS_System_ComponentModel.MaskedTextResultHint MaskedTextResultHint_out = MS_System_ComponentModel.MaskedTextResultHint.Unknown;

			foreach (string mask in test_masks) {
				foreach (string str in string_values) {
					bool more_states = true;
					int stateindex = 0;
					do {
						object [] arguments;
						arguments = new object [] { str, Int32_out, MaskedTextResultHint_out };
						if (Compare ("VerifyString", mask, ref stateindex, arguments, ref more_states)) {
							continue;
						}

						WriteTestStart ();
						mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
						WriteLine ("mtp = new MaskedTextProvider (@\"" + mask.Replace ("\"", "\"\"") + "\");");
						more_states = CreateState (mtp, stateindex);
						stateindex++;

						WriteLine ("Assert.AreEqual ({0}, mtp.VerifyString ({1}, out Int32_out, out MaskedTextResultHint_out), \"#{2}\");",
							GetStringValue (mtp.VerifyString (str, out Int32_out, out MaskedTextResultHint_out)), GetStringValue (str), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, MaskedTextResultHint_out, \"#{1}\");", GetStringValue (MaskedTextResultHint_out), (counter++).ToString ());
						WriteLine ("Assert.AreEqual ({0}, Int32_out, \"#{1}\");", GetStringValue (Int32_out), (counter++).ToString ());

						WriteAssertProperties (mtp, Name, TestName, ref counter);
						WriteTestEnd ();
					} while (more_states);
				}
			}

			WriteTestFooter ();
		}

		private static void CreateObjects (out object mtp_MS, out object mtp_Mono, string mask)
		{
			if (type_MS == null) {
				type_MS = Assembly.Load ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").GetType ("System.ComponentModel.MaskedTextProvider");
				type_Hint_MS = Assembly.Load ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").GetType ("System.ComponentModel.MaskedTextResultHint");
			}
			if (type_Mono == null) {
				type_Mono = Assembly.GetExecutingAssembly ().GetType ("System.ComponentModel.MaskedTextProvider");
				type_Hint_Mono = Assembly.GetExecutingAssembly ().GetType ("System.ComponentModel.MaskedTextResultHint");
			}

			if (type_MS == null) {
				Assert.Ignore ("Could not load MS' version of MaskedTextProvider.");
			}
			if (type_Mono == null) {
				Assert.Ignore ("Could not load Mono's version of MaskedTextProvider.");
			}
			if (type_Mono == type_MS) {
				Assert.Ignore ("You're running on the Mono runtime, this test can only be run on the MS runtime.");
			}

			mtp_Mono = type_Mono.GetConstructor (new Type [] { typeof (string) }).Invoke (new object [] { mask });
			mtp_MS = type_MS.GetConstructor (new Type [] { typeof (string) }).Invoke (new object [] { mask });
		}

		// Gets the mtp into a certain state. 
		// returns false if index+1 is not valid.
		// index starts at 0.
		static bool CreateState (object mtp, int index)
		{
			object [] states = (object [])state_methods_values [index];
			for (int j = 0; j < states.Length; j++) {
				object [] state = (object [])states [j];
				string name = (string)state [0];
				object [] args = (object [])state [1];
				Type [] arg_types = new Type [args.Length];
				for (int i = 0; i < args.Length; i++) {
					arg_types [i] = args [i].GetType ();
				}
				MethodInfo method = mtp.GetType ().GetMethod (name, arg_types);
				if (method == null) {
					if (arg_types [arg_types.Length - 1].Name == "MaskedTextResultHint") {
						arg_types [arg_types.Length - 1] = arg_types [arg_types.Length - 1].MakeByRefType ();
						arg_types [arg_types.Length - 2] = arg_types [arg_types.Length - 2].MakeByRefType ();
						args [arg_types.Length - 1] = (int)args [arg_types.Length - 1];
						method = mtp.GetType ().GetMethod (name, arg_types);
						args [arg_types.Length - 1] = Enum.ToObject (method.GetParameters () [arg_types.Length - 1].ParameterType.GetElementType (), (int)args [arg_types.Length - 1]);
					}
				}
				if (method == null)
					Console.WriteLine ("STOP");
				method.Invoke (mtp, args);
				string line;
				line = "mtp." + method.Name + "(";
				for (int i = 0; i < args.Length; i++) {
					if (arg_types [i].IsByRef) {
						line += "out " + arg_types [i].GetElementType ().Name + "_out";
					} else {
						line += GetStringValue (args [i]);
					}
					if (i < args.Length - 1)
						line += ", ";
				}
				line += ");";
				WriteLine (line);
			}
			if (state_methods_values.Length <= index + 1)
				return false;
			return true;
		}

		static void WriteLineNonFormat (string msg)
		{
			if (dont_write)
				return;

			writer.WriteLine (tabs + msg);
		}
		static void WriteLine (string msg, params string [] args)
		{
			if (dont_write)
				return;
			writer.WriteLine (tabs + msg, args);
		}

		static string tabs
		{
			get
			{
				return new string ('\t', tab);
			}
		}

		static Array GetTestValues (Type tp)
		{
			if (tp == typeof (char)) {
				return char_values;
			} else if (tp == typeof (int) || tp == typeof (int).MakeByRefType ()) {
				return int_values;
			} else if (tp == typeof (string)) {
				return string_values;
			} else if (tp == typeof (CultureInfo)) {
				return culture_infos;
			} else if (tp == typeof (bool)) {
				return new bool [] { true, false };
			} else if (tp == typeof (MaskedTextResultHint) || tp == typeof (MaskedTextResultHint).MakeByRefType ()) {
				return hint_values;
			} else if (tp == typeof (object)) {
				return object_values;
			} else {
				throw new NotImplementedException ();
			}
		}

		static string GetStringValue (object obj)
		{
			if (obj == null)
				return "null";

			Type tp = obj.GetType ();

			if (tp == typeof (char)) {
				return string.Format ("'\\x{0:X}'", Convert.ToInt32 ((char)obj));
			} else if (tp == typeof (int)) {
				return obj.ToString ();
			} else if (tp == typeof (string)) {
				return "@\"" + obj.ToString ().Replace ("\"", "\"\"") + "\"";
			} else if (tp == typeof (CultureInfo)) {
				CultureInfo ci = (CultureInfo)obj;
				//return "\"" + ci.Name + "\"";
				return "CultureInfo.GetCultureInfo (\"" + ci.Name + "\")";
			} else if (tp == typeof (bool)) {
				return ((bool)obj) ? "true" : "false";
			} else if (tp == typeof (MaskedTextProvider)) {
				return "@\"" + obj.ToString ().Replace ("\"", "\"\"") + "\"";
			} else if (tp is IEnumerator) {
				return "@\"" + obj.ToString ().Replace ("\"", "\"\"") + "\"";
			} else if (tp == typeof (List<int>.Enumerator)) {
				return "@\"" + obj.ToString ().Replace ("\"", "\"\"") + "\"";
			} else if (tp.Name == "MaskedTextResultHint") {
				return "MaskedTextResultHint." + obj.ToString ();
			} else if (tp is Type) {
				return "typeof (" + ((Type)obj).FullName + ")";
			} else {
				throw new NotImplementedException ();
			}
		}

		static bool IncIndex (int [] indices, Array [] inputs)
		{
			for (int i = indices.Length - 1; i >= 0; i--) {
				if (indices [i] >= inputs [i].Length - 1) {
					if (i == 0) {
						return false;
					}
					indices [i] = 0;
					indices [i - 1]++;

					int a, b;
					a = indices [i - 1];
					b = inputs [i - 1].Length - 1;
					if (a < b) {
						return true;
					}

				} else {
					indices [i]++;
					return true;
				}
			}
			return false;
		}

		static string GetTestName (string prefix, ParameterInfo [] ps)
		{
			string result = prefix;
			for (int b = 0; b < ps.Length; b++)
				result += "_" + ps [b].ParameterType.Name.Replace ("&", "").Replace ("+", "").Replace ("*", "");
			result += "_Test";
			return result;

		}

		//static void MethodsTest ()
		//{
		//        string Name = "mtp";
		//        int counter = 0;
		//        MS_System_ComponentModel.MaskedTextProvider mtp = null;

		//        for (int a = 0; a < methods.Length; a++) {
		//                MethodInfo method = methods [a];

		//                if (method.Name.StartsWith ("get_") || method.Name.StartsWith ("set_"))
		//                        continue;
		//                if (method.IsStatic)
		//                        continue;

		//                ParameterInfo [] ps = method.GetParameters ();
		//                Array [] inputs = new Array [ps.Length];
		//                int [] indices = new int [ps.Length];

		//                string TestName = GetTestName (method.Name, ps);

		//                Console.WriteLine ("Method (" + (a + 1).ToString () + "/" + methods.Length.ToString () + "): " + TestName);

		//                int assert_count = 1;
		//                WriteTestHeader (TestName);
		//                WriteLine ("MaskedTextProvider mtp;");
		//                WriteLine ("object result = null;");


		//                for (int i = 0; i < ps.Length; i++)
		//                        inputs [i] = GetTestValues (ps [i].ParameterType);

		//                foreach (string mask in test_masks) {
		//                        do {
		//                                Exception ex = null;
		//                                object result = null;

		//                                object [] args = new object [inputs.Length];
		//                                for (int i = 0; i < inputs.Length; i++) {
		//                                        args [i] = inputs [i].GetValue (indices [i]);
		//                                }

		//                                mtp = new MS_System_ComponentModel.MaskedTextProvider (mask);
		//                                WriteLine ("mtp = new MaskedTextProvider (@\"{0}\");", mask.Replace ("\"", "\"\""));

		//                                try {
		//                                        result = method.Invoke (mtp, args);
		//                                } catch (TargetInvocationException e) {
		//                                        ex = e.InnerException;
		//                                } catch (Exception e) {
		//                                        ex = e;
		//                                }

		//                                WriteLine ("");
		//                                WriteLine ("try {");
		//                                tab++;
		//                                string tmp = "";
		//                                for (int j = 0; j < args.Length; j++) {
		//                                        bool tmpvar = false;
		//                                        if (tmp != "")
		//                                                tmp += ", ";
		//                                        //if (ps [j].ParameterType.IsByRef) {
		//                                        //        tmp += "ref ";
		//                                        //        tmpvar = true;
		//                                        //}
		//                                        if (ps [j].IsOut) {
		//                                                tmp += "out ";
		//                                                tmpvar = true;
		//                                        }
		//                                        if (tmpvar) {
		//                                                string name = "tmpvar_" + (counter++).ToString ();
		//                                                WriteLine (ps [j].ParameterType.GetElementType ().Name + " " + name + " = " + GetStringValue (args [j]) + ";");
		//                                                tmp += name;
		//                                        } else {
		//                                                tmp += GetStringValue (args [j]);
		//                                        }
		//                                }
		//                                string statement;
		//                                bool is_void = !(method.ReturnType == null || method.ReturnType == typeof (void));
		//                                statement = "mtp." + method.Name + " (" + tmp + ");";
		//                                if (is_void) {
		//                                        statement = "result = " + statement;
		//                                }
		//                                WriteLine (statement);

		//                                if (ex != null) {
		//                                        WriteLine ("Assert.Fail (\"Expected '{0}'\");", ex.GetType ().FullName);
		//                                } else {
		//                                        WriteAssertProperties (mtp, Name, TestName, ref assert_count);
		//                                        if (!is_void)
		//                                                WriteLine ("Assert.AreEqual ({0}, result, \"{1}#{2}\");", GetStringValue (result), TestName, (assert_count++).ToString ());
		//                                }
		//                                tab--;
		//                                WriteLine ("} catch (Exception ex) {");
		//                                tab++;
		//                                if (ex == null) {
		//                                        WriteLine ("Assert.Fail (\"Unexpected exception of Type = \" + ex.GetType ().FullName + \", Message = \" + ex.Message + \".\");");
		//                                } else {
		//                                        WriteLine ("Assert.AreEqual (\"{0}\", ex.GetType ().FullName, \"{1}#{2}\");", ex.GetType ().FullName, TestName, (assert_count++).ToString ());
		//                                        WriteLine ("Assert.AreEqual (@\"{0}\", ex.Message, \"{1}#{2}\");", ex.Message.Replace ("\"", "\"\""), TestName, (assert_count++).ToString ());
		//                                }
		//                                tab--;
		//                                WriteLine ("}");

		//                        } while (IncIndex (indices, inputs));
		//                        WriteLine ("");
		//                }
		//                WriteTestFooter ();
		//        }

		//}

		//static void ConstructorTest ()
		//{
		//        int assert_count = 1;
		//        string Name = "mtp";


		//        foreach (ConstructorInfo ctor in ctors) {
		//                string TestName = GetTestName ("Constructor", ctor.GetParameters ());

		//                WriteTestHeader (TestName);
		//                MS_System_ComponentModel.MaskedTextProvider mtp = null;
		//                WriteLine ("MaskedTextProvider mtp;");

		//                ParameterInfo [] ps = ctor.GetParameters ();
		//                Array [] inputs = new Array [ps.Length];
		//                int [] indices = new int [ps.Length];

		//                for (int i = 0; i < ps.Length; i++)
		//                        inputs [i] = GetTestValues (ps [i].ParameterType);

		//                do {
		//                        object [] args = new object [inputs.Length];
		//                        for (int i = 0; i < inputs.Length; i++) {
		//                                args [i] = inputs [i].GetValue (indices [i]);
		//                        }

		//                        Exception ex = null;
		//                        mtp = null;
		//                        try {
		//                                mtp = (MS_System_ComponentModel.MaskedTextProvider)ctor.Invoke (args);
		//                        } catch (TargetInvocationException e) {
		//                                ex = e.InnerException;
		//                        } catch (Exception e) {
		//                                ex = e;
		//                        }

		//                        WriteLine ("");
		//                        WriteLine ("try {");
		//                        tab++;
		//                        string tmp = "";
		//                        for (int j = 0; j < args.Length; j++) {
		//                                if (tmp != "")
		//                                        tmp += ", ";
		//                                tmp += GetStringValue (args [j]);
		//                        }

		//                        string statement;
		//                        statement = "mtp = new MaskedTextProvider (" + tmp + ");";
		//                        WriteLine (statement);
		//                        if (ex == null) {
		//                                ok_constructors.Add (ctor);
		//                                ok_constructors_args.Add (args);
		//                                ok_constructors_statements.Add (statement);
		//                        }

		//                        if (ex != null) {
		//                                WriteLine ("Assert.Fail (\"Expected '{0}'\");", ex.GetType ().FullName);
		//                        } else {
		//                                WriteAssertProperties (mtp, Name, TestName, ref assert_count);
		//                        }
		//                        tab--;
		//                        WriteLine ("} catch (Exception ex) {");
		//                        tab++;
		//                        if (ex == null) {
		//                                WriteLine ("Assert.Fail (\"Unexpected exception of Type = \" + ex.GetType ().FullName + \", Message = \" + ex.Message + \".\");");
		//                        } else {
		//                                WriteLine ("Assert.AreEqual (\"{0}\", ex.GetType ().FullName, \"{1}#{2}\");", ex.GetType ().FullName, TestName, (assert_count++).ToString ());
		//                                WriteLine ("Assert.AreEqual (@\"{0}\", ex.Message, \"{1}#{2}\");", ex.Message.Replace ("\"", "\"\""), TestName, (assert_count++).ToString ());
		//                        }
		//                        tab--;
		//                        WriteLine ("}");

		//                } while (IncIndex (indices, inputs));
		//                WriteTestFooter ();
		//        }

		//}

		static string current_test_name;
		static int current_test_counter;
		static int skipped_test_counter;
		static int total_skipped_counter;
		static int total_test_counter;
		static string [] current_test_method_init;
		static void WriteTestHeader (string TestName, params string [] method_init)
		{
			//WriteLine ("[Test]");
			//WriteLine ("public void " + TestName  + " ()");
			WriteLineNonFormat ("[TestFixture]");
			WriteLineNonFormat ("public class " + TestName);
			WriteLineNonFormat ("{");
			tab++;
			total_test_counter += current_test_counter;
			current_test_counter = 0;
			total_skipped_counter += skipped_test_counter;
			skipped_test_counter = 0;
			current_test_name = TestName;
			current_test_method_init = method_init;
			Console.Write ("Writing " + TestName + "... ");
		}
		static void WriteTestFooter ()
		{
			tab--;
			WriteLineNonFormat ("}");
			Console.WriteLine ("written " + (current_test_counter - skipped_test_counter).ToString () + " tests, and skipped " + skipped_test_counter.ToString () + " tests.");
			dont_write = false;
		}
		static void WriteTestStart ()
		{
			current_test_counter++;

			WriteLineNonFormat ("[Test]");
			WriteLineNonFormat ("public void " + current_test_name + current_test_counter.ToString ("00000") + " ()");
			WriteLineNonFormat ("{");
			tab++;
			foreach (string str in current_test_method_init)
				WriteLine (str);
		}
		static void WriteTestEnd ()
		{
			tab--;
			WriteLineNonFormat ("}");
		}
		static void WriteFileHeader ()
		{
			WriteLineNonFormat (
@"//
// System.ComponentModel.MaskedTextProvider generated test cases
//
// Authors:
//      Rolf Bjarne Kvinge (RKvinge@novell.com)
// 
// (c) 2007 Novell
// 

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class MaskedTextProviderGeneratedTests
	{
");

			tab += 2;
		}
		static void WriteFileFooter ()
		{
			WriteLineNonFormat (
@"	
	}
}
");
			tab -= 2;
			Console.WriteLine ("Written " + (total_test_counter - total_skipped_counter).ToString () + " tests in total and skipped " + total_skipped_counter.ToString () + " tests.");
		}

		//static void WriteAssertProperties (MaskedTextProvider mtp, string Name, string TestName, ref int i)
		//{
		//        WriteLine ("// Testing all properties...");
		//        //return;
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AllowPromptAsInput, \"{2}-#{3}\");", GetStringValue (mtp.AllowPromptAsInput), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AsciiOnly, \"{2}-#{3}\");", GetStringValue (mtp.AsciiOnly), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AssignedEditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.AssignedEditPositionCount), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AvailableEditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.AvailableEditPositionCount), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.Culture, \"{2}-#{3}\");", GetStringValue (mtp.Culture), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.EditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.EditPositionCount), Name, TestName, (i++).ToString ());
		//        //WriteLine ("Assert.AreEqual ({0}, {1}.EditPositions, \"{2}-#{3}\");", GetStringValue (mtp.EditPositions), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IncludeLiterals, \"{2}-#{3}\");", GetStringValue (mtp.IncludeLiterals), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IncludePrompt, \"{2}-#{3}\");", GetStringValue (mtp.IncludePrompt), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IsPassword, \"{2}-#{3}\");", GetStringValue (mtp.IsPassword), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.LastAssignedPosition, \"{2}-#{3}\");", GetStringValue (mtp.LastAssignedPosition), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.Length, \"{2}-#{3}\");", GetStringValue (mtp.Length), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.Mask, \"{2}-#{3}\");", GetStringValue (mtp.Mask), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.MaskCompleted, \"{2}-#{3}\");", GetStringValue (mtp.MaskCompleted), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.MaskFull, \"{2}-#{3}\");", GetStringValue (mtp.MaskFull), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.PasswordChar, \"{2}-#{3}\");", GetStringValue (mtp.PasswordChar), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.PromptChar, \"{2}-#{3}\");",GetStringValue ( mtp.PromptChar), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.ResetOnPrompt, \"{2}-#{3}\");", GetStringValue (mtp.ResetOnPrompt), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.ResetOnSpace, \"{2}-#{3}\");", GetStringValue (mtp.ResetOnSpace), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.SkipLiterals, \"{2}-#{3}\");", GetStringValue (mtp.SkipLiterals), Name, TestName, (i++).ToString ());
		//}


		static bool CompareMaskTextProviders (object mtp_MS, object mtp_Mono)
		{
			object value_ms, value_mono;
			PropertyInfo [] fields = mtp_MS.GetType ().GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
			foreach (PropertyInfo field_MS in fields) {
				if (field_MS.Name == "EditPositions")
					continue;
				if (field_MS.Name == "Culture")
					continue;
				if (field_MS.Name == "Item")
					continue;

				PropertyInfo field_Mono = mtp_Mono.GetType ().GetProperty (field_MS.Name);

				value_ms = field_MS.GetValue (mtp_MS, null);
				value_mono = field_Mono.GetValue (mtp_Mono, null);

				if (!Compare (value_ms, value_mono)) {
					return false;
				}
			}

			MethodInfo method_MS, method_Mono;
			string name;
			Type [] args;
			object [] all_values = new object [] {
				new object [] {},
				new object [] {true},
				new object [] {false},
				new object [] {true, true},
				new object [] {false, true},
				new object [] {true, false},
				new object [] {false, false},
			};

			name = "ToString";
			foreach (object [] values in all_values) {
				args = new Type [values.Length];
				for (int i = 0; i < values.Length; i++) {
					args [i] = values [i].GetType ();
				}
				method_Mono = mtp_Mono.GetType ().GetMethod (name, args);
				method_MS = mtp_MS.GetType ().GetMethod (name, args);
				value_ms = method_MS.Invoke (mtp_MS, values);
				value_mono = method_Mono.Invoke (mtp_Mono, values);
				if (!Compare (value_ms, value_mono)) {
					return false;
				}
			}

			return true;
		}

		static bool Compare (object v1, object v2)
		{
			if (v1 == null && v2 == null) {
				return true;
			} else if (v1 == null ^ v2 == null) {
				return false;
			} else if (v1.GetType ().Name == "MaskedTextResultHint" && v2.GetType ().Name == "MaskedTextResultHint") {
				return (int)v1 == (int)v2;
			} else if (v1.GetType ().FullName == "System.Collections.Generic.List`1+Enumerator[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]" && v2.GetType ().FullName == v1.GetType ().FullName) {
				List <int>.Enumerator list1, list2;
				list1 = (List<int>.Enumerator) v1;
				list2 = (List<int>.Enumerator) v2;
				int item1, item2;
				bool next1, next2;
				do {
					next1 = list1.MoveNext ();
					next2 = list2.MoveNext ();
					if (next1 ^ next2) {
						return false;
					}
					if (!next1 && !next2) {
						return true;
					}
					item1 = list1.Current;
					item2 = list2.Current;
					if (item1 != item2) {
						return false;
					}
				} while (true);
			} else if (!v1.Equals (v2)) {
				return false;
			} else {
				return true;
			}
		}
		static bool Compare (string methodName, string mask, ref int stateindex, object [] args, ref bool more_states)
		{
			//if (!new StackFrame (1).GetMethod ().Name.Contains (methodName) && !new StackFrame (2).GetMethod ().Name.Contains (methodName)) {
			//        Console.WriteLine ("STOP");
			//        Console.Read ();
			//}

			bool result = false;

			try {
				if ((current_test_counter - skipped_test_counter) > MAXFAILEDTESTS) {
					more_states = false;
					return true;
				}

				object [] args_MS, args_Mono;
				object mtp_MS, mtp_Mono;
				dont_write = true;

				args_MS = new object [args.Length];
				args_Mono = new object [args.Length];

				CreateObjects (out mtp_MS, out mtp_Mono, mask);

				for (int i = 0; i < args.Length; i++) {
					if (args [i] != null && args [i].GetType ().Name == "MaskedTextResultHint") {
						args_Mono [i] = Enum.ToObject (type_Hint_Mono, (int)args [i]);
						args_MS [i] = Enum.ToObject (type_Hint_MS, (int)args [i]);
					} else {
						args_Mono [i] = args [i];
						args_MS [i] = args [i];
					}
				}

				more_states = CreateState (mtp_MS, stateindex);
				more_states = CreateState (mtp_Mono, stateindex);

				if (mtp_MS.GetType ().GetProperty (methodName) != null) {
					methodName = "get_" + methodName;
				}
				object result_MS, result_Mono;
				result_MS = mtp_MS.GetType ().InvokeMember (methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mtp_MS, args_MS);
				result_Mono = mtp_Mono.GetType ().InvokeMember (methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mtp_Mono, args_Mono);

				result = true;
				for (int arg = 0; arg < args_Mono.Length; arg++) {
					if (!Compare (args_MS [arg], args_Mono [arg])) {
						result = false;
						break;
					}
				}
				if (result && !CompareMaskTextProviders (mtp_MS, mtp_Mono)) {
					result = false;
				}
				if (result && !Compare (result_MS, result_Mono)) {
					result = false;
				}
			} catch (Exception ex) {
				result = false;
				more_states = false;
				Console.WriteLine (ex.Message);
			} finally {
				dont_write = false;

				if (result) {
					current_test_counter++;
					skipped_test_counter++;
					stateindex++;
				}
			}
			return result;
		}

		static void WriteAssertPropertiesMethod ()
		{
			string filecontents = File.ReadAllText (Path.Combine (Path.GetDirectoryName (Path.GetFullPath (destination_file)), "MaskedTextProviderTest.cs"));
			string method;
			int start, end;
			start = filecontents.IndexOf ("/*" + " START */"); // strings are split in two so to not match itself.
			end = filecontents.IndexOf ("/*" + " END */");
			method = filecontents.Substring (start + 11, end - start - 11);
			WriteLineNonFormat (method.Replace ("{", "{").Replace ("}", "}"));

			//public static void AssertProperties (MaskedTextProvider mtp, string test_name, int counter, bool allow_prompt, bool ascii_only, int assigned_edit_position_count, int available_edit_position_count, 
			//                CultureInfo culture, int edit_position_count, bool include_literals, bool include_prompt, bool is_password, int last_assigned_position, 
			//                int length, string mask, bool mask_completed, bool mask_full, char password_char, char prompt_char, bool reset_on_prompt, bool reset_on_space, bool skip_literals, 
			//                string tostring, string tostring_true, string tostring_false, string tostring_true_true, string tostring_true_false, string tostring_false_true, string tostring_false_false)
			//                {
			//                // Testing all properties...
			//                //return;
			//                int i = 1;
			//                Assert.AreEqual (allow_prompt, mtp.AllowPromptAsInput, string.Format(""{0}-#{1} (AllowPromptAsInput)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (ascii_only, mtp.AsciiOnly, string.Format(""{0}-#{1} (AsciiOnly)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (assigned_edit_position_count, mtp.AssignedEditPositionCount, string.Format(""{0}-#{1} (AssignedEditPositionCount)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (available_edit_position_count, mtp.AvailableEditPositionCount, string.Format(""{0}-#{1} (AvailableEditPositionCount)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (culture, mtp.Culture, string.Format(""{0}-#{1} (Culture)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (edit_position_count, mtp.EditPositionCount, string.Format(""{0}-#{1} (EditPositionCount)"", test_name + counter.ToString (), (i++).ToString ()));
			//                //Assert.AreEqual ({0}, mtp.EditPositions,string.Format( ""{0}-#{1} (EditPositions)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (include_literals, mtp.IncludeLiterals, string.Format(""{0}-#{1} (IncludeLiterals)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (include_prompt, mtp.IncludePrompt, string.Format(""{0}-#{1} (IncludePrompt)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (is_password, mtp.IsPassword, string.Format(""{0}-#{1} (IsPassword)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (last_assigned_position, mtp.LastAssignedPosition, string.Format(""{0}-#{1} (LastAssignedPosition)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (length, mtp.Length, string.Format(""{0}-#{1} (Length)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (mask, mtp.Mask, string.Format(""{0}-#{1} (Mask)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (mask_completed, mtp.MaskCompleted, string.Format(""{0}-#{1} (MaskCompleted)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (mask_full, mtp.MaskFull, string.Format(""{0}-#{1} (MaskFull)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (password_char, mtp.PasswordChar, string.Format(""{0}-#{1} (PasswordChar)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (prompt_char, mtp.PromptChar, string.Format(""{0}-#{1} (PromptChar)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (reset_on_prompt, mtp.ResetOnPrompt, string.Format(""{0}-#{1} (ResetOnPrompt)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (reset_on_space, mtp.ResetOnSpace, string.Format(""{0}-#{1} (ResetOnSpace)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (skip_literals, mtp.SkipLiterals, string.Format(""{0}-#{1} (SkipLiterals)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring, mtp.ToString (), string.Format(""{0}-#{1} (tostring)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_true, mtp.ToString (true), string.Format(""{0}-#{1} (tostring_true)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_false, mtp.ToString (false), string.Format(""{0}-#{1} (tostring_false)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_true_true, mtp.ToString (true, true), string.Format(""{0}-#{1} (tostring_true_true)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_true_false, mtp.ToString (true, false), string.Format(""{0}-#{1} (tostring_true_false)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_false_true, mtp.ToString (false, true), string.Format(""{0}-#{1} (tostring_false_true)"", test_name + counter.ToString (), (i++).ToString ()));
			//                Assert.AreEqual (tostring_false_false, mtp.ToString (false, false), string.Format(""{0}-#{1} (tostring_false_false)"", test_name + counter.ToString (), (i++).ToString ()));

			//                }
			//        ".Replace ("{", "{").Replace ("}", "}"));
		}

		static void WriteAssertProperties (MS_System_ComponentModel.MaskedTextProvider mtp, string Name, string TestName, ref int i)
		{
			StringBuilder call = new StringBuilder ();
			call.Append ("MaskedTextProviderTest.AssertProperties (mtp, \"" + TestName + "\", " + (i++).ToString ());
			call.Append (", " + GetStringValue (mtp.AllowPromptAsInput));
			call.Append (", " + GetStringValue (mtp.AsciiOnly));
			call.Append (", " + GetStringValue (mtp.AssignedEditPositionCount));
			call.Append (", " + GetStringValue (mtp.AvailableEditPositionCount));
			call.Append (", " + GetStringValue (mtp.Culture));
			call.Append (", " + GetStringValue (mtp.EditPositionCount));
			//call.Append (", " + GetStringValue (mtp.EditPositions));
			call.Append (", " + GetStringValue (mtp.IncludeLiterals));
			call.Append (", " + GetStringValue (mtp.IncludePrompt));
			call.Append (", " + GetStringValue (mtp.IsPassword));
			call.Append (", " + GetStringValue (mtp.LastAssignedPosition));
			call.Append (", " + GetStringValue (mtp.Length));
			call.Append (", " + GetStringValue (mtp.Mask));
			call.Append (", " + GetStringValue (mtp.MaskCompleted));
			call.Append (", " + GetStringValue (mtp.MaskFull));
			call.Append (", " + GetStringValue (mtp.PasswordChar));
			call.Append (", " + GetStringValue (mtp.PromptChar));
			call.Append (", " + GetStringValue (mtp.ResetOnPrompt));
			call.Append (", " + GetStringValue (mtp.ResetOnSpace));
			call.Append (", " + GetStringValue (mtp.SkipLiterals));
			call.Append (", " + QuoteString (mtp.ToString ()));
			call.Append (", " + QuoteString (mtp.ToString (true)));
			call.Append (", " + QuoteString (mtp.ToString (false)));
			call.Append (", " + QuoteString (mtp.ToString (true, true)));
			call.Append (", " + QuoteString (mtp.ToString (true, false)));
			call.Append (", " + QuoteString (mtp.ToString (false, true)));
			call.Append (", " + QuoteString (mtp.ToString (false, false)));
			call.Append (");");
			WriteLine (call.ToString ());
		}

		static string QuoteString (string str)
		{
			return "@\"" + str.Replace ("\"", "\"\"") + "\"";
		}

		//static void WriteAssertProperties2 (MS_System_ComponentModel.MaskedTextProvider mtp, string Name, string TestName, ref int i)
		//{
		//        WriteLine ("// Testing all properties...");
		//        //return;
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AllowPromptAsInput, \"{2}-#{3}\");", GetStringValue (mtp.AllowPromptAsInput), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AsciiOnly, \"{2}-#{3}\");", GetStringValue (mtp.AsciiOnly), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AssignedEditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.AssignedEditPositionCount), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.AvailableEditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.AvailableEditPositionCount), Name, TestName, (i++).ToString ());
		//        //WriteLine ("Assert.AreEqual ({0}, {1}.Culture, \"{2}-#{3}\");", GetStringValue (mtp.Culture), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.EditPositionCount, \"{2}-#{3}\");", GetStringValue (mtp.EditPositionCount), Name, TestName, (i++).ToString ());
		//        //WriteLine ("Assert.AreEqual ({0}, {1}.EditPositions, \"{2}-#{3}\");", GetStringValue (mtp.EditPositions), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IncludeLiterals, \"{2}-#{3}\");", GetStringValue (mtp.IncludeLiterals), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IncludePrompt, \"{2}-#{3}\");", GetStringValue (mtp.IncludePrompt), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.IsPassword, \"{2}-#{3}\");", GetStringValue (mtp.IsPassword), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.LastAssignedPosition, \"{2}-#{3}\");", GetStringValue (mtp.LastAssignedPosition), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.Length, \"{2}-#{3}\");", GetStringValue (mtp.Length), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.Mask, \"{2}-#{3}\");", GetStringValue (mtp.Mask), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.MaskCompleted, \"{2}-#{3}\");", GetStringValue (mtp.MaskCompleted), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.MaskFull, \"{2}-#{3}\");", GetStringValue (mtp.MaskFull), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.PasswordChar, \"{2}-#{3}\");", GetStringValue (mtp.PasswordChar), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.PromptChar, \"{2}-#{3}\");", GetStringValue (mtp.PromptChar), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.ResetOnPrompt, \"{2}-#{3}\");", GetStringValue (mtp.ResetOnPrompt), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.ResetOnSpace, \"{2}-#{3}\");", GetStringValue (mtp.ResetOnSpace), Name, TestName, (i++).ToString ());
		//        WriteLine ("Assert.AreEqual ({0}, {1}.SkipLiterals, \"{2}-#{3}\");", GetStringValue (mtp.SkipLiterals), Name, TestName, (i++).ToString ());
		//}
	}
}