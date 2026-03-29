window.scheduleShareImage = {
    downloadSvgAsPng: async function (elementId, fileName) {
        const image = document.getElementById(elementId);

        if (!image) {
            return;
        }

        const draw = function () {
            const width = image.naturalWidth || image.width;
            const height = image.naturalHeight || image.height;

            if (!width || !height) {
                return;
            }

            const canvas = document.createElement("canvas");
            canvas.width = width * 2;
            canvas.height = height * 2;

            const context = canvas.getContext("2d");

            if (!context) {
                return;
            }

            context.scale(2, 2);
            context.drawImage(image, 0, 0, width, height);

            const pngUrl = canvas.toDataURL("image/png");
            const link = document.createElement("a");
            link.href = pngUrl;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        };

        if (image.complete) {
            draw();
            return;
        }

        image.onload = draw;
    }
};
