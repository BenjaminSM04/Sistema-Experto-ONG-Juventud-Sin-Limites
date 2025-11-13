// Función para descargar archivos
window.downloadFile = function (filename, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(link.href);
};

// Función para descargar archivo desde URL
window.downloadFileFromUrl = function (url, filename) {
    const link = document.createElement('a');
  link.href = url;
    link.download = filename || 'download';
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Función para descargar archivo desde base64
window.downloadFileFromBase64 = function (base64Data, filename, mimeType) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
 const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });
    
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(link.href);
};

// Función para imprimir contenido HTML
window.printHtml = function (htmlContent) {
    const printWindow = window.open('', '_blank');
    printWindow.document.write(htmlContent);
  printWindow.document.close();
    printWindow.focus();
    
    printWindow.onload = function () {
        printWindow.print();
    };
};

// Función para mostrar confirmación antes de eliminar
window.confirmDelete = function (message) {
    return confirm(message || '¿Está seguro de que desea eliminar este elemento?');
};

// Función para scroll suave a un elemento
window.scrollToElement = function (elementId) {
  const element = document.getElementById(elementId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

// Función para copiar al portapapeles
window.copyToClipboard = function (text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
    return navigator.clipboard.writeText(text);
 } else {
        // Fallback para navegadores antiguos
        const textArea = document.createElement('textarea');
        textArea.value = text;
  textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
  textArea.select();
        try {
    document.execCommand('copy');
    document.body.removeChild(textArea);
   return Promise.resolve();
        } catch (err) {
        document.body.removeChild(textArea);
    return Promise.reject(err);
      }
    }
};
