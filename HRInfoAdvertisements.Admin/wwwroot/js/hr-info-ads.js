window.hrInfoAds = window.hrInfoAds || {};

window.hrInfoAds.scrollToFeedback = function () {

    const element =
        document.getElementById("my-ads-feedback");

    if (!element) {
        return;
    }

    element.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });

    setTimeout(function () {

        element.classList.add(
            "feedback-highlight"
        );

        setTimeout(function () {

            element.classList.remove(
                "feedback-highlight"
            );

        }, 1600);

    }, 300);
};