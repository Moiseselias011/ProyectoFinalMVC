;
(function () {
    'use strict';

    async function obtenerHeaders() {
        const headers = { 'Content-Type': 'application/json' };
        if (window.TokenApi) {
            Object.assign(headers, await window.TokenApi.cabeceraAutorizacion());
        }
        return headers;
    }

    async function llamarLike(url, darLike) {
        const respuesta = await fetch(url, {
            method: darLike ? 'POST' : 'DELETE',
            headers: await obtenerHeaders(),
            credentials: 'omit'
        });

        let datos = null;
        try {
            datos = await respuesta.json();
        } catch {
            datos = null;
        }

        if (!respuesta.ok) {
            const mensaje = datos && (datos.mensaje || datos.error)
                ? (datos.mensaje || datos.error)
                : 'No se pudo actualizar el Me gusta.';
            throw new Error(mensaje);
        }

        return datos || {};
    }

    function clasificarBoton(boton, leGusta) {
        boton.classList.toggle('btn-danger', leGusta);
        boton.classList.toggle('btn-outline-danger', leGusta);
        boton.classList.toggle('btn-primary', !leGusta);
        boton.classList.toggle('btn-outline-primary', !leGusta);
        boton.textContent = leGusta ? 'Ya no me gusta' : 'Me gusta';
    }

    document.addEventListener('click', async function (e) {
        let boton = null;

        if (e.target.matches('[data-like-tutorial]')) {
            boton = e.target;
            const idTutorial = parseInt(boton.getAttribute('data-like-tutorial'), 10);
            if (!idTutorial) return;

            const darLike = boton.getAttribute('data-le-gusta') !== 'true';
            boton.disabled = true;

            try {
                const datos = await llamarLike(`/api/likes/tutoriales/${idTutorial}`, darLike);
                boton.setAttribute('data-le-gusta', darLike ? 'true' : 'false');
                clasificarBoton(
                    boton,
                    darLike
                );

                const contador = document.querySelector('[data-total-likes-tutorial]');
                if (contador && datos.totalLikes !== undefined) {
                    contador.textContent = `${datos.totalLikes} Me gusta`;
                }
            } catch (error) {
                console.error('Error en like:', error);
                window.alert(error.message);
            } finally {
                boton.disabled = false;
            }
            return;
        }

        if (e.target.matches('[data-like-comentario]')) {
            boton = e.target;
            const idComentario = parseInt(boton.getAttribute('data-like-comentario'), 10);
            if (!idComentario) return;

            const darLike = boton.getAttribute('data-le-gusta') !== 'true';
            boton.disabled = true;

            try {
                const datos = await llamarLike(`/api/likes/comentarios/${idComentario}`, darLike);
                boton.setAttribute('data-le-gusta', darLike ? 'true' : 'false');
                clasificarBoton(boton, darLike);

                const contador = document.querySelector(
                    `[data-total-likes-comentario="${idComentario}"]`
                );
                if (contador && datos.totalLikes !== undefined) {
                    contador.textContent = `${datos.totalLikes} Me gusta`;
                }
            } catch (error) {
                console.error('Error en like de comentario:', error);
                window.alert(error.message);
            } finally {
                boton.disabled = false;
            }
        }
    });
})();
