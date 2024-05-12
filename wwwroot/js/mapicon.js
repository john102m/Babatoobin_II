async function myMap() {

    var myCenter = new google.maps.LatLng(55.82965333259990, -4.516478061676025);
    var mapCanvas = document.getElementById("map");
    var mapOptions = { center: myCenter, zoom: 12 };
    var map = new google.maps.Map(mapCanvas, mapOptions);
    //var marker = new google.maps.Marker(
    //    {
    //        position: myCenter,
    //        icon: "img/favicon.png",
    //        title: "Babatoobins, busy bee",
    //    });

    //marker.setMap(map);

    const infoWindow = new google.maps.InfoWindow({
        content: document.getElementById('infobox-content').innerHTML,
        ariaLabel: "Babatoobins",
    });

    marker.addListener("click", () => {
        infoWindow.open({ anchor: marker, map, });

    });
}