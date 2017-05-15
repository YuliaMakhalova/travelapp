function setBodyBottomPadding(){
	$(".site_wrap").css("min-height", $(window).height() );

	var padding = $(".site-footer").outerHeight(true);
	$('.site-footer').prev().css("margin-bottom", padding + 30);
}
	
jQuery(document).ready(function($) {

	'use strict';

		$(".our-listing").owlCarousel({
			items: 4,
			navigation: true,
			navigationText: ["&larr;","&rarr;"],
		});


		$('.flexslider').flexslider({
		    animation: "fade",
		    controlNav: false,
		    prevText: "&larr;",
		    nextText: "&rarr;"
		});


		$('.toggle-menu').click(function(){
	        $('.menu-responsive').slideToggle();
	        return false;
	    });

		setBodyBottomPadding();

		$(window).on("resize", function(){
			setBodyBottomPadding();
		});

});









