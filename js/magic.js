var temp = 0;

$(document).ready(function() {
	setImageOne();
	
	$("#downloadBtn").click(function() {
		$("#downloadBtn").attr("disabled", "disabled");
		$("#downloadBtn").text("Your download will start shortly. Enjoy!");
		window.location.href = "http://hypereddie.com/ECPL/mango.zip";
	});

	function setImageOne() {
		$("#pp").fadeIn(800).attr('src', 'img/1.jpg').delay(2500).fadeOut(800, function() { setImageTwo() });
	}
	
	function setImageTwo() {
		$("#pp").fadeIn(800).attr('src', 'img/2.jpg').delay(2500).fadeOut(800, function() { setImageThree() });
	}
	
	function setImageThree() {
		$("#pp").fadeIn(800).attr('src', 'img/3.jpg').delay(2500).fadeOut(800, function() { setImageOne() });
	}
});