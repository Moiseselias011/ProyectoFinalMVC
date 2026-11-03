(function () {
    'use strict';

    let tokenCacheado = null;
    let peticionEnCurso = null;

    async function obtenerToken() {
        if (tokenCacheado) return tokenCacheado;
        if (peticionEnCurso) return peticionEnCurso;

        peticionEnCurso = fetch('/Auth/Token', { credentials: 'same-origin' })
            .then(function (respuesta) {
                if (!respuesta.ok) {
                    tokenCacheado = null;
                    return null;
                }
                return respuesta.json();
            })
            .then(function (datos) {
                if (datos && datos.token) {
                    tokenCacheado = datos.token;
                } else {
                    tokenCacheado = null;
                }
                return tokenCacheado;
            })
            .catch(function () {
                tokenCacheado = null;
                return null;
            })
            .finally(function () {
                peticionEnCurso = null;
            });

        return peticionEnCurso;
    }

    async function cabeceraAutorizacion() {
        const token = await obtenerToken();
        if (!token) return {};
        return { 'Authorization': 'Bearer ' + token };
    }

    window.TokenApi = {
        obtenerToken: obtenerToken,
        cabeceraAutorizacion: cabeceraAutorizacion
    };
})();
