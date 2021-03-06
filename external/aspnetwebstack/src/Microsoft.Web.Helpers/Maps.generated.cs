#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.WebPages.Html;
    using System.Diagnostics;
    using System.Web.WebPages.Scope;
    using System.Web.UI.WebControls;
    using System.Globalization;
    using Microsoft.Internal.Web.Utils;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorSingleFileGenerator", "0.6.0.0")]
    public class Maps : System.Web.WebPages.HelperPage
    {
#line hidden

    private const string DefaultWidth = "300px";
    private const string DefaultHeight = "300px";
    private static readonly object _mapIdKey = new object();
    private static readonly object _mapQuestApiKey = new object();
    private static readonly object _bingApiKey = new object();
    private static readonly object _yahooApiKey = new object();

    public static string MapQuestApiKey {
        get {
            return (string)ScopeStorage.CurrentScope[_mapQuestApiKey];
        }
        set {
            ScopeStorage.CurrentScope[_mapQuestApiKey] = value;
        }
    }

    public static string YahooApiKey {
        get {
            return (string)ScopeStorage.CurrentScope[_yahooApiKey];
        }
        set {
            ScopeStorage.CurrentScope[_yahooApiKey] = value;
        }
    }

    public static string BingApiKey {
        get {
            return (string)ScopeStorage.CurrentScope[_bingApiKey];
        }
        set {
            ScopeStorage.CurrentScope[_bingApiKey] = value;
        }
    }
    
    private static int MapId {
         get {
            var value = (int?)HttpContext.Current.Items[_mapIdKey];
            return value ?? 0;
        }
        set {
            HttpContext.Current.Items[_mapIdKey] = value;
        }
    }
    
    private static string GetMapElementId() {
        return "map_" + MapId;   
    }
    
    private static string TryParseUnit(string value, string defaultValue) {
        if (String.IsNullOrEmpty(value)) {
            return defaultValue;
        }
        try {
            return Unit.Parse(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        } catch (ArgumentException) {
            return defaultValue;    
        }    
    }

    private static IHtmlString RawJS(string text) {
        return Raw(HttpUtility.JavaScriptStringEncode(text));
    }
    
    private static IHtmlString Raw(string text) {
        return new HtmlString(text);    
    }
    
    private static string GetApiKey(string apiKey, object scopeStorageKey) {
        if (apiKey.IsEmpty()) {
            return (string)ScopeStorage.CurrentScope[scopeStorageKey];   
        }
        return apiKey;   
    }

    public class MapLocation {
        private readonly string _latitude;
        private readonly string _longitude;
        public MapLocation(string latitude, string longitude) {
            _latitude = latitude;
            _longitude = longitude;
        }

        public string Latitude {
            get { return _latitude; }
        }

        public string Longitude {
            get { return _longitude; }
        }
    }

    internal static string GetDirectionsQuery(string location, string latitude, string longitude, Func<string, string> encoder = null) {
        encoder = encoder ?? HttpUtility.UrlEncode;
        Debug.Assert(!(location.IsEmpty() && latitude.IsEmpty() && longitude.IsEmpty()));
        if (location.IsEmpty()) {
            return encoder(latitude + "," + longitude);
        }
        return encoder(location);
    }
#line hidden
public static System.Web.WebPages.HelperResult GetMapQuestHtml(string key = null, string location = null, string latitude = null, string longitude = null, string width = "300px", string height = "300px", int zoom = 7, string type = "map",
            bool showDirectionsLink = true, string directionsLinkText = "Get Directions", bool showZoomControl = true, IEnumerable<MapLocation> pushpins = null) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {


                                                                                                                                                                  
    key = GetApiKey(key, _mapQuestApiKey);
    if (key.IsEmpty()) {
        throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");  
    }
    
    string mapElement = GetMapElementId();
    string loc = "null"; // We want to print the value 'null' in the client
    if (latitude != null && longitude != null) {
        loc = String.Format(CultureInfo.InvariantCulture, "{{lat: {0}, lng: {1}}}",
            HttpUtility.JavaScriptStringEncode(latitude, addDoubleQuotes: false), HttpUtility.JavaScriptStringEncode(longitude, addDoubleQuotes: false));
    }

    // The MapQuest key listed on their website is Url encoded to begin with. 

WriteLiteralTo(@__razor_helper_writer, "    <script src=\"http://mapquestapi.com/sdk/js/v6.0.0/mqa.toolkit.js?key=");


                                          WriteTo(@__razor_helper_writer, key);

WriteLiteralTo(@__razor_helper_writer, "\" type=\"text/javascript\"></script>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <script type=\"text/javascript\">\r\n        MQA.EventUtil.observe(window, \'load\'" +
", function() {\r\n            var map = new MQA.TileMap(document.getElementById(\'");


                                WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\'), ");


                                               WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ", ");


                                                      WriteTo(@__razor_helper_writer, Raw(loc));

WriteLiteralTo(@__razor_helper_writer, ", \'");


                                                                  WriteTo(@__razor_helper_writer, RawJS(type));

WriteLiteralTo(@__razor_helper_writer, "\'); \r\n");


             if (showZoomControl) {

WriteLiteralTo(@__razor_helper_writer, "            ");

WriteLiteralTo(@__razor_helper_writer, "\r\n                MQA.withModule(\'zoomcontrol3\', function() {\r\n\t                m" +
"ap.addControl(new MQA.LargeZoomControl3(), new MQA.MapCornerPlacement(MQA.MapCor" +
"ner.TOP_LEFT));\r\n                });\r\n            ");

WriteLiteralTo(@__razor_helper_writer, "\r\n");


            }


             if (!String.IsNullOrEmpty(location)) {

WriteLiteralTo(@__razor_helper_writer, "            ");

WriteLiteralTo(@__razor_helper_writer, "\r\n                MQA.withModule(\'geocoder\', function() {\r\n                    ma" +
"p.geocodeAndAddLocations(\'");


                 WriteTo(@__razor_helper_writer, RawJS(location));

WriteLiteralTo(@__razor_helper_writer, "\');\r\n                });\r\n            ");

WriteLiteralTo(@__razor_helper_writer, "\r\n");


            }


             if (pushpins != null) {
                foreach (var p in pushpins) {

WriteLiteralTo(@__razor_helper_writer, "                    ");

WriteLiteralTo(@__razor_helper_writer, " map.addShape(new MQA.Poi({lat:");


                      WriteTo(@__razor_helper_writer, RawJS(p.Latitude));

WriteLiteralTo(@__razor_helper_writer, ",lng:");


                                             WriteTo(@__razor_helper_writer, RawJS(p.Longitude));

WriteLiteralTo(@__razor_helper_writer, "}));\r\n");


	            }
            }

WriteLiteralTo(@__razor_helper_writer, "        });\r\n    </script>\r\n");


    

WriteLiteralTo(@__razor_helper_writer, "    <div id=\"");


WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\" style=\"width:");


        WriteTo(@__razor_helper_writer, TryParseUnit(width, DefaultWidth));

WriteLiteralTo(@__razor_helper_writer, "; height:");


                                                   WriteTo(@__razor_helper_writer, TryParseUnit(height, DefaultHeight));

WriteLiteralTo(@__razor_helper_writer, ";\">\r\n    </div>\r\n");


    if (showDirectionsLink) {

WriteLiteralTo(@__razor_helper_writer, "        <a class=\"map-link\" href=\"http://www.mapquest.com/?q=");


                              WriteTo(@__razor_helper_writer, GetDirectionsQuery(location, latitude, longitude));

WriteLiteralTo(@__razor_helper_writer, "\">");


                                                                                  WriteTo(@__razor_helper_writer, directionsLinkText);

WriteLiteralTo(@__razor_helper_writer, "</a>\r\n");


    }
    MapId++;

});

}

#line hidden
public static System.Web.WebPages.HelperResult GetBingHtml(string key = null, string location = null, string latitude = null, string longitude = null, string width = null, string height = null, int zoom = 14, string type = "auto",
            bool showDirectionsLink = true, string directionsLinkText = "Get Directions", IEnumerable<MapLocation> pushpins = null) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {


                                                                                                                                     
    key = GetApiKey(key, _bingApiKey);
    if (key.IsEmpty()) {
        throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
    } 
    string mapElement = GetMapElementId();
    
    type = (type ?? "auto").ToLowerInvariant();


WriteLiteralTo(@__razor_helper_writer, "    <script src=\"http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0" +
"\" type=\"text/javascript\"></script>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <script type=\"text/javascript\">\r\n        jQuery(window).load(function() { \r\n " +
"           var map = new Microsoft.Maps.Map(document.getElementById(\"");


                                       WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\"), { credentials: \'");


                                                                      WriteTo(@__razor_helper_writer, RawJS(key));

WriteLiteralTo(@__razor_helper_writer, "\', mapTypeId: Microsoft.Maps.MapTypeId[\'");


                                                                                                                         WriteTo(@__razor_helper_writer, RawJS(type));

WriteLiteralTo(@__razor_helper_writer, "\'] });\r\n");


             if (latitude != null && longitude != null) {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, " map.setView({zoom: ");


       WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ", center: new Microsoft.Maps.Location(");


                                                  WriteTo(@__razor_helper_writer, RawJS(latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                                                    WriteTo(@__razor_helper_writer, RawJS(longitude));

WriteLiteralTo(@__razor_helper_writer, ")});\r\n");


            }
            else if (location != null) {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, "\r\n                map.getCredentials(function(credentials) {\r\n                   " +
" $.ajax(\'http://dev.virtualearth.net/REST/v1/Locations/");


                                           WriteTo(@__razor_helper_writer, RawJS(location));

WriteLiteralTo(@__razor_helper_writer, @"', {
                        data: { output: 'json', key: credentials }, dataType: 'json', jsonp: 'jsonp',
                        success: function(data) {
                            if (data && data.resourceSets && data.resourceSets.length > 0 && data.resourceSets[0].resources && data.resourceSets[0].resources.length > 0) {
                                var r = data.resourceSets[0].resources[0].point.coordinates;
                                var loc = new Microsoft.Maps.Location(r[0], r[1]);
                                map.setView({zoom: ");


                    WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ", center: loc});              \r\n                                map.entities.push" +
"(new Microsoft.Maps.Pushpin(loc, null));\r\n                            }\r\n       " +
"                 }\r\n                    });\r\n                });\r\n              " +
"  ");

WriteLiteralTo(@__razor_helper_writer, "\r\n");


            }


             if (pushpins != null) {
                foreach(var loc in pushpins) {

WriteLiteralTo(@__razor_helper_writer, "                    ");

WriteLiteralTo(@__razor_helper_writer, " map.entities.push(new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(");


                                                                 WriteTo(@__razor_helper_writer, RawJS(loc.Latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                                                                       WriteTo(@__razor_helper_writer, RawJS(loc.Longitude));

WriteLiteralTo(@__razor_helper_writer, "), null));\r\n");


                }
            }

WriteLiteralTo(@__razor_helper_writer, "        });\r\n    </script>\r\n");


    

WriteLiteralTo(@__razor_helper_writer, "    <div class=\"map\" id=\"");


WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\" style=\"position:relative; width:");


                                       WriteTo(@__razor_helper_writer, TryParseUnit(width, DefaultWidth));

WriteLiteralTo(@__razor_helper_writer, "; height:");


                                                                                  WriteTo(@__razor_helper_writer, TryParseUnit(height, DefaultHeight));

WriteLiteralTo(@__razor_helper_writer, ";\">\r\n    </div>\r\n");


    if (showDirectionsLink) {
        // Review: Need to figure out if the link needs to be localized. 

WriteLiteralTo(@__razor_helper_writer, "        <a class=\"map-link\" href=\"http://www.bing.com/maps/?v=2&where1=");


                                        WriteTo(@__razor_helper_writer, GetDirectionsQuery(location, latitude, longitude));

WriteLiteralTo(@__razor_helper_writer, "\">");


                                                                                            WriteTo(@__razor_helper_writer, directionsLinkText);

WriteLiteralTo(@__razor_helper_writer, "</a>\r\n");


    }
    MapId++;

});

}

#line hidden
public static System.Web.WebPages.HelperResult GetGoogleHtml(string location = null, string latitude = null, string longitude = null, string width = null, string height = null, int zoom = 14, string type = "ROADMAP",
                bool showDirectionsLink = true, string directionsLinkText = "Get Directions", IEnumerable<MapLocation> pushpins = null) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {


                                                                                                                                         
    string mapElement = GetMapElementId();
    type = (type ?? "ROADMAP").ToUpperInvariant(); // Map types are in upper case

    // Google maps does not support null centers. We'll set it to arbitrary values if they are null and only the location is provided.
    // These locations are somewhere around Microsoft's Redmond Campus.
    latitude = latitude ?? "47.652437";
    longitude = longitude ?? "-122.132424";


WriteLiteralTo(@__razor_helper_writer, "    <script src=\"http://maps.google.com/maps/api/js?sensor=false\" type=\"text/java" +
"script\"></script>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <script type=\"text/javascript\">\r\n        $(function() {\r\n            var map " +
"= new google.maps.Map(document.getElementById(\"");


                                    WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\"), { zoom: ");


                                                           WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ", center: new google.maps.LatLng(");


                                                                                                 WriteTo(@__razor_helper_writer, RawJS(latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                                                                                                   WriteTo(@__razor_helper_writer, RawJS(longitude));

WriteLiteralTo(@__razor_helper_writer, "), mapTypeId: google.maps.MapTypeId[\'");


                                                                                                                                                                         WriteTo(@__razor_helper_writer, RawJS(type));

WriteLiteralTo(@__razor_helper_writer, "\'] });\r\n");


             if (!String.IsNullOrEmpty(location)) {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, "\r\n                new google.maps.Geocoder().geocode({address: \'");


                               WriteTo(@__razor_helper_writer, RawJS(location));

WriteLiteralTo(@__razor_helper_writer, @"'}, function(response, status) {
                    if (status === google.maps.GeocoderStatus.OK) {
                        var best = response[0].geometry.location;
                        map.panTo(best);
                        new google.maps.Marker({map : map, position: best });
                    }
                });
                ");

WriteLiteralTo(@__razor_helper_writer, "\r\n");


            }


             if (pushpins != null) {
                foreach(var loc in pushpins) {

WriteLiteralTo(@__razor_helper_writer, "                    ");

WriteLiteralTo(@__razor_helper_writer, " new google.maps.Marker({map : map, position: new google.maps.LatLng(");


                                                            WriteTo(@__razor_helper_writer, RawJS(loc.Latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                                                                  WriteTo(@__razor_helper_writer, RawJS(loc.Longitude));

WriteLiteralTo(@__razor_helper_writer, ")});\r\n");


                }
            }

WriteLiteralTo(@__razor_helper_writer, "        });\r\n    </script>\r\n");


    

WriteLiteralTo(@__razor_helper_writer, "    <div class=\"map\" id=\"");


WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\" style=\"width:");


                    WriteTo(@__razor_helper_writer, TryParseUnit(width, DefaultWidth));

WriteLiteralTo(@__razor_helper_writer, "; height:");


                                                               WriteTo(@__razor_helper_writer, TryParseUnit(height, DefaultHeight));

WriteLiteralTo(@__razor_helper_writer, ";\">\r\n    </div>\r\n");


    if (showDirectionsLink) {

WriteLiteralTo(@__razor_helper_writer, "        <a class=\"map-link\" href=\"http://maps.google.com/maps?q=");


                                 WriteTo(@__razor_helper_writer, GetDirectionsQuery(location, latitude, longitude));

WriteLiteralTo(@__razor_helper_writer, "\">");


                                                                                     WriteTo(@__razor_helper_writer, directionsLinkText);

WriteLiteralTo(@__razor_helper_writer, "</a>\r\n");


    }
    MapId++;

});

}

#line hidden
public static System.Web.WebPages.HelperResult GetYahooHtml(string key = null, string location = null, string latitude = null, string longitude = null, string width = null, string height = null, int zoom = 4, string type = "YAHOO_MAP_REG",
                bool showDirectionsLink = true, string directionsLinkText = "Get Directions", IEnumerable<MapLocation> pushpins = null) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {


                                                                                                                                         
    key = GetApiKey(key, _yahooApiKey);
    if (key.IsEmpty()) {
        throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
    }                     
    string mapElement = GetMapElementId();


WriteLiteralTo(@__razor_helper_writer, "    <script src=\"http://api.maps.yahoo.com/ajaxymap?v=3.8&appid=");


                                 WriteTo(@__razor_helper_writer, HttpUtility.UrlEncode(key));

WriteLiteralTo(@__razor_helper_writer, "\" type=\"text/javascript\"></script>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <script type=\"text/javascript\">\r\n        $(function() {\r\n            var map " +
"= new YMap(document.getElementById(\'");


                         WriteTo(@__razor_helper_writer, RawJS(mapElement));

WriteLiteralTo(@__razor_helper_writer, "\'));  \r\n            map.addTypeControl();  \r\n            map.setMapType(");


WriteTo(@__razor_helper_writer, RawJS(type));

WriteLiteralTo(@__razor_helper_writer, ");  \r\n");


             if (latitude != null && longitude != null) {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, " map.drawZoomAndCenter(new YGeoPoint(");


                        WriteTo(@__razor_helper_writer, RawJS(latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                          WriteTo(@__razor_helper_writer, RawJS(longitude));

WriteLiteralTo(@__razor_helper_writer, "), ");


                                                              WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ");\r\n");


            }
            else if (!String.IsNullOrEmpty(location)) {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, " map.drawZoomAndCenter(\'");


           WriteTo(@__razor_helper_writer, RawJS(location));

WriteLiteralTo(@__razor_helper_writer, "\', ");


                              WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ");\r\n");


            }
            else {

WriteLiteralTo(@__razor_helper_writer, "                ");

WriteLiteralTo(@__razor_helper_writer, " map.setZoomLevel(");


     WriteTo(@__razor_helper_writer, zoom);

WriteLiteralTo(@__razor_helper_writer, ");\r\n");


            }


             if(pushpins != null) {
                foreach (var loc in pushpins) {

WriteLiteralTo(@__razor_helper_writer, "                     ");

WriteLiteralTo(@__razor_helper_writer, " map.addMarker(new YGeoPoint(");


                     WriteTo(@__razor_helper_writer, RawJS(loc.Latitude));

WriteLiteralTo(@__razor_helper_writer, ", ");


                                           WriteTo(@__razor_helper_writer, RawJS(loc.Longitude));

WriteLiteralTo(@__razor_helper_writer, "));\r\n");


                 }
            }

WriteLiteralTo(@__razor_helper_writer, "\r\n        });\r\n    </script>\r\n");


    

WriteLiteralTo(@__razor_helper_writer, "    <div id=\"");


WriteTo(@__razor_helper_writer, mapElement);

WriteLiteralTo(@__razor_helper_writer, "\" style=\"width:");


        WriteTo(@__razor_helper_writer, TryParseUnit(width, DefaultWidth));

WriteLiteralTo(@__razor_helper_writer, "; height:");


                                                   WriteTo(@__razor_helper_writer, TryParseUnit(height, DefaultHeight));

WriteLiteralTo(@__razor_helper_writer, ";\">\r\n    </div>\r\n");


    if (showDirectionsLink) {

WriteLiteralTo(@__razor_helper_writer, "        <a class=\"map-link\" href=\"http://maps.yahoo.com/#q1=");


                             WriteTo(@__razor_helper_writer, GetDirectionsQuery(location, latitude, longitude, HttpUtility.UrlPathEncode));

WriteLiteralTo(@__razor_helper_writer, "\">");


                                                                                                            WriteTo(@__razor_helper_writer, directionsLinkText);

WriteLiteralTo(@__razor_helper_writer, "</a>\r\n");


    }
    MapId++;

});

}


        public Maps()
        {
        }
        protected static System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
    }
}
#pragma warning restore 1591
