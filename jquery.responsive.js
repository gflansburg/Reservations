;(function ($) {
    $(document).ready(function () {
        var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
        pageRequestManager.add_endRequest(function () {
            addResizeHandler();
        });

        addResizeHandler();
    })

    function addResizeHandler() {
        $(window).resize(function () {
            resize();
        })

        resize();
    }

    function resize() {
        var width = $(".Gafware_Modules_Reservations").width();

        //console.log(width);

        if (width < 905) {
            $(".Gafware_Modules_Reservations_Step").addClass("Gafware_Modules_Reservations_Step_900");
        }
        else {
            $(".Gafware_Modules_Reservations_Step").removeClass("Gafware_Modules_Reservations_Step_900");
        }

        if (width < 485) {
            $(".Gafware_Modules_Reservations_Step").addClass("Gafware_Modules_Reservations_Step_480");
        }
        else {
            $(".Gafware_Modules_Reservations_Step").removeClass("Gafware_Modules_Reservations_Step_480");
        }
    }
})(jQuery);