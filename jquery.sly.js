;(function ($) {
    $(document).ready(function () {
        var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
        pageRequestManager.add_endRequest(function () {
            initializeHorizontalScrollers();
        });

        initializeHorizontalScrollers();
    })

    var slies = new Array();

    function initializeHorizontalScrollers() {
        slies = new Array();

        $(".Gafware_Modules_Reservations_HorizontalScroll_Items").each(function (index, value) {
            var sly = new Sly($(value), {
                horizontal: 1,
                itemNav: 'basic',
                smart: 1,
                mouseDragging: 1,
                touchDragging: 1,
                releaseSwing: 1,
                startAt: 0,
                scrollBy: 1,
                speed: 200,
                elasticBounds: 1,
                easing: 'swing',
                prevPage: $(value).parent().find('.Gafware_Modules_Reservations_LessCommandButton'),
                nextPage: $(value).parent().find('.Gafware_Modules_Reservations_MoreCommandButton')
            });

            sly.init();
            sly.activate($(value).find(".Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"));
            sly.reload();

            slies.push(sly);
        });

        $(window).resize(function () {
            $(slies).each(function (index, value) {
                value.reload();
                value.activate($(value.frame).find(".Gafware_Modules_Reservations_HorizontalScroll_Item_Selected"));
            });
        })
    }
})(jQuery);