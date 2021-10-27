;(function ($) {
    $(document).ready(function () {
        var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
        pageRequestManager.add_endRequest(function () {
            postAuthorizeNetSIMForm();
        });

        postAuthorizeNetSIMForm();
    })

    function postAuthorizeNetSIMForm() {
        if($(".Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden").length > 0){
            var decoded = $("<div/>").html($(".Gafware_Modules_Reservations_AuthorizeNetSIMForm_Hidden").val()).text();
            $(decoded).appendTo("body").submit();
        }
    }
})(jQuery);