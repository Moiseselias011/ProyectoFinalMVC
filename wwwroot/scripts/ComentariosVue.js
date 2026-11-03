;
(function () {
    'use strict';

    const MAX_CONTENIDO = 1000;

    const plantilla = `
<div v-cloak>
    <div v-if="mensaje" :class="'alert alert-' + mensaje.tipo + ' alert-dismissible mb-3'" role="alert">
        <span v-text="mensaje.texto"></span>
        <button type="button" class="btn-close" @click="mensaje = null"></button>
    </div>

    <div class="card mb-3">
        <div class="card-body">
            <h6 class="card-subtitle mb-2 text-muted">Escribir un comentario</h6>
            <textarea v-model="nuevoContenido" class="form-control" rows="3" maxlength="1000"
                      placeholder="Escribe un comentario..."></textarea>
            <small class="text-muted d-block mt-1" v-text="nuevoContenido.length + '/1000'"></small>
            <button type="button" class="btn btn-primary mt-2" @click="crearComentario()" :disabled="cargando">Comentar</button>
        </div>
    </div>

    <div v-if="cargando" class="text-muted mb-3">Cargando comentarios...</div>

    <div v-if="!cargando && comentarios.length === 0" class="alert alert-info">
        Todavía no hay comentarios en tiempo real. ¡Sé el primero en comentar!
    </div>

    <div v-for="c in comentarios" :key="c.idcomentario" class="border rounded p-3 mb-3">
        <div class="d-flex justify-content-between">
            <strong v-text="(c.usuario && c.usuario.nombreUsuario) || 'Anónimo'"></strong>
            <small class="text-muted" v-text="formatearFecha(c.fechaCreacion)"></small>
        </div>

        <p v-if="editandoId !== c.idcomentario" class="mt-2 mb-2" v-text="c.contenido"></p>
        <template v-else>
            <textarea v-model="contenidoEdicion" class="form-control mb-2" rows="3" maxlength="1000"></textarea>
            <div class="mb-2">
                <button type="button" class="btn btn-sm btn-primary" @click="guardarEdicion()">Guardar cambios</button>
                <button type="button" class="btn btn-sm btn-outline-secondary" @click="cancelarEdicion()">Cancelar</button>
            </div>
        </template>

        <div class="d-flex gap-2 align-items-center flex-wrap">
            <span class="badge bg-info text-dark" v-if="c.likes" v-text="c.likes.length + ' Me gusta'"></span>

            <button v-if="estaAutenticado" type="button" class="btn btn-sm"
                    :class="leGusta(c) ? 'btn-danger' : 'btn-outline-primary'"
                    :data-le-gusta="leGusta(c)"
                    @click="alternarLike(c)"
                    v-text="leGusta(c) ? 'Ya no me gusta' : 'Me gusta'"></button>

            <button v-if="estaAutenticado" type="button" class="btn btn-sm btn-outline-success"
                    :class="{ 'd-none': formRespuestaAbierto === c.idcomentario }"
                    @click="alternarFormularioRespuesta(c)">Responder</button>

            <button v-if="puedeEditar(c) && editandoId !== c.idcomentario"
                    type="button" class="btn btn-sm btn-outline-secondary"
                    @click="iniciarEdicion(c)">Editar</button>

            <button v-if="puedeEliminar(c)" type="button" class="btn btn-sm btn-outline-danger"
                    @click="eliminar(c)">Eliminar</button>

            <span v-if="c.respuestas && c.respuestas.length" class="badge bg-light text-dark border"
                  v-text="c.respuestas.length + ' respuesta(s)'"></span>
        </div>

        <div v-if="formRespuestaAbierto === c.idcomentario" class="mt-3">
            <textarea v-model="contenidoRespuesta" class="form-control mb-2" rows="2" maxlength="1000"
                      placeholder="Escribe una respuesta..."></textarea>
            <button type="button" class="btn btn-sm btn-success" @click="responder(c)">Responder</button>
            <button type="button" class="btn btn-sm btn-outline-secondary" @click="cerrarFormularioRespuesta()">Cancelar</button>
        </div>

        <div v-for="r in c.respuestas" :key="r.idcomentario" class="border-start ps-3 mt-3">
            <div class="d-flex justify-content-between">
                <strong v-text="(r.usuario && r.usuario.nombreUsuario) || 'Anónimo'"></strong>
                <small class="text-muted" v-text="formatearFecha(r.fechaCreacion)"></small>
            </div>

            <p v-if="editandoId !== r.idcomentario" class="mt-1 mb-1" v-text="r.contenido"></p>
            <template v-else>
                <textarea v-model="contenidoEdicion" class="form-control mb-2" rows="2" maxlength="1000"></textarea>
                <div class="mb-2">
                    <button type="button" class="btn btn-sm btn-primary" @click="guardarEdicion()">Guardar cambios</button>
                    <button type="button" class="btn btn-sm btn-outline-secondary" @click="cancelarEdicion()">Cancelar</button>
                </div>
            </template>

            <div class="d-flex gap-2 align-items-center flex-wrap">
                <span v-if="r.likes" class="text-muted" v-text="r.likes.length + ' Me gusta'"></span>

                <button v-if="estaAutenticado" type="button" class="btn btn-sm"
                        :class="leGusta(r) ? 'btn-danger' : 'btn-outline-primary'"
                        :data-le-gusta="leGusta(r)"
                        @click="alternarLike(r)"
                        v-text="leGusta(r) ? 'Ya no me gusta' : 'Me gusta'"></button>

                <button v-if="puedeEditar(r) && editandoId !== r.idcomentario"
                        type="button" class="btn btn-sm btn-outline-secondary"
                        @click="iniciarEdicion(r)">Editar</button>

                <button v-if="puedeEliminar(r)" type="button" class="btn btn-sm btn-outline-danger"
                        @click="eliminar(r)">Eliminar</button>
            </div>
        </div>
    </div>

    <nav v-if="totalPaginas > 1">
        <ul class="pagination pagination-sm">
            <li class="page-item" :class="{ disabled: paginaActual <= 1 }">
                <button type="button" class="page-link" @click="cargarComentarios(paginaActual - 1)">Anterior</button>
            </li>
            <li v-for="i in totalPaginas" :key="'pag' + i" class="page-item" :class="{ active: i === paginaActual }">
                <button type="button" class="page-link" @click="cargarComentarios(i)" v-text="i"></button>
            </li>
            <li class="page-item" :class="{ disabled: paginaActual >= totalPaginas }">
                <button type="button" class="page-link" @click="cargarComentarios(paginaActual + 1)">Siguiente</button>
            </li>
        </ul>
    </nav>
</div>
`;

    const componente = {
        template: plantilla,
        data: function () {
            const contenedor = document.getElementById('app-comentarios');
            return {
                idTutorial: parseInt(contenedor ? contenedor.getAttribute('data-tutorial-id') : '0', 10) || 0,
                estaAutenticado: !!contenedor && contenedor.getAttribute('data-autenticado') === 'true',
                usuarioId: parseInt(contenedor ? contenedor.getAttribute('data-usuario-id') : '0', 10) || 0,
                esAdministrador: !!contenedor && contenedor.getAttribute('data-es-admin') === 'true',

                comentarios: [],
                nuevoContenido: '',
                editandoId: null,
                contenidoEdicion: '',
                formRespuestaAbierto: null,
                contenidoRespuesta: '',
                cargando: false,
                mensaje: null,
                paginaActual: 1,
                totalPaginas: 1,
                tamanioPagina: 10
            };
        },
        mounted: function () {
            this.cargarComentarios(1);
        },
        methods: {
            formatearFecha(iso) {
                if (!iso) return '';
                const fecha = new Date(iso);
                return Number.isNaN(fecha.getTime()) ? '' : fecha.toLocaleString();
            },
            leGusta(comentario) {
                const likes = comentario.likes || [];
                return likes.some(l => l.usuarioId === this.usuarioId);
            },
            puedeEditar(comentario) {
                return (this.usuarioId !== 0 && comentario.idusuario === this.usuarioId) || this.esAdministrador;
            },
            puedeEliminar(comentario) {
                return this.puedeEditar(comentario);
            },
            mostrarAlerta(tipo, texto) {
                this.mensaje = { tipo: tipo, texto: texto };
                const self = this;
                window.setTimeout(function () {
                    if (self.mensaje && self.mensaje.texto === texto) {
                        self.mensaje = null;
                    }
                }, 5000);
            },
            async cabeceraAutorizacion() {
                return window.TokenApi ? await window.TokenApi.cabeceraAutorizacion() : {};
            },

            async errorDeRespuesta(respuesta, porDefecto) {
                let mensaje = porDefecto;
                try {
                    const datos = await respuesta.json();
                    if (datos && (datos.mensaje || datos.error)) {
                        mensaje = datos.mensaje || datos.error;
                    }
                } catch {
                }
                return mensaje;
            },

            async cargarComentarios(pagina) {
                if (this.idTutorial <= 0) return;
                if (!pagina || pagina < 1) pagina = 1;
                this.paginaActual = pagina;
                this.cargando = true;
                try {
                    const respuesta = await fetch(`/api/comentarios/${this.idTutorial}?pagina=${pagina}`);
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'Error al cargar los comentarios.'));
                    }
                    const datos = await respuesta.json();
                    this.comentarios = datos.comentarios || [];
                    this.totalPaginas = datos.totalPaginas || 1;
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                } finally {
                    this.cargando = false;
                }
            },

            async crearComentario() {
                if (!this.estaAutenticado) {
                    this.mostrarAlerta('danger', 'Inicia sesión para comentar.');
                    return;
                }
                const contenido = this.nuevoContenido.trim();
                if (!contenido) {
                    this.mostrarAlerta('danger', 'El comentario es obligatorio.');
                    return;
                }
                if (contenido.length > MAX_CONTENIDO) {
                    this.mostrarAlerta('danger', 'El comentario no puede superar los 1000 caracteres.');
                    return;
                }
                this.cargando = true;
                try {
                    const respuesta = await fetch('/api/comentarios', {
                        method: 'POST',
                        headers: Object.assign({ 'Content-Type': 'application/json' }, await this.cabeceraAutorizacion()),
                        credentials: 'omit',
                        body: JSON.stringify({ idTutorial: this.idTutorial, contenido: contenido })
                    });
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'No se pudo crear el comentario.'));
                    }
                    this.nuevoContenido = '';
                    this.mostrarAlerta('success', 'Comentario creado correctamente.');
                    await this.cargarComentarios(1);
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                } finally {
                    this.cargando = false;
                }
            },

            alternarFormularioRespuesta(comentario) {
                if (this.formRespuestaAbierto === comentario.idcomentario) {
                    this.formRespuestaAbierto = null;
                    this.contenidoRespuesta = '';
                    return;
                }
                this.formRespuestaAbierto = comentario.idcomentario;
                this.contenidoRespuesta = '';
            },
            cerrarFormularioRespuesta() {
                this.formRespuestaAbierto = null;
                this.contenidoRespuesta = '';
            },
            async responder(comentario) {
                const contenido = this.contenidoRespuesta.trim();
                if (!contenido) {
                    this.mostrarAlerta('danger', 'La respuesta es obligatoria.');
                    return;
                }
                if (contenido.length > MAX_CONTENIDO) {
                    this.mostrarAlerta('danger', 'La respuesta no puede superar los 1000 caracteres.');
                    return;
                }
                this.cargando = true;
                try {
                    const respuesta = await fetch(`/api/comentarios/${comentario.idcomentario}/responder`, {
                        method: 'POST',
                        headers: Object.assign({ 'Content-Type': 'application/json' }, await this.cabeceraAutorizacion()),
                        credentials: 'omit',
                        body: JSON.stringify({
                            idTutorial: this.idTutorial,
                            contenido: contenido
                        })
                    });
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'No se pudo responder el comentario.'));
                    }
                    this.cerrarFormularioRespuesta();
                    this.mostrarAlerta('success', 'Respuesta creada correctamente.');
                    await this.cargarComentarios(this.paginaActual);
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                } finally {
                    this.cargando = false;
                }
            },

            iniciarEdicion(comentario) {
                this.editandoId = comentario.idcomentario;
                this.contenidoEdicion = comentario.contenido;
            },
            cancelarEdicion() {
                this.editandoId = null;
                this.contenidoEdicion = '';
            },
            async guardarEdicion() {
                const contenido = this.contenidoEdicion.trim();
                if (!contenido) {
                    this.mostrarAlerta('danger', 'El contenido es obligatorio.');
                    return;
                }
                if (contenido.length > MAX_CONTENIDO) {
                    this.mostrarAlerta('danger', 'El contenido no puede superar los 1000 caracteres.');
                    return;
                }
                this.cargando = true;
                try {
                    const respuesta = await fetch(`/api/comentarios/${this.editandoId}`, {
                        method: 'PUT',
                        headers: Object.assign({ 'Content-Type': 'application/json' }, await this.cabeceraAutorizacion()),
                        credentials: 'omit',
                        body: JSON.stringify({ contenido: contenido })
                    });
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'No se pudo editar el comentario.'));
                    }
                    const comentarioEditado = this.buscarComentario(this.editandoId);
                    if (comentarioEditado) comentarioEditado.contenido = contenido;
                    this.cancelarEdicion();
                    this.mostrarAlerta('success', 'Comentario actualizado.');
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                } finally {
                    this.cargando = false;
                }
            },

            async eliminar(comentario) {
                if (!window.confirm('¿Seguro que deseas eliminar este comentario?')) return;
                this.cargando = true;
                try {
                    const respuesta = await fetch(`/api/comentarios/${comentario.idcomentario}`, {
                        method: 'DELETE',
                        headers: Object.assign({ 'Content-Type': 'application/json' }, await this.cabeceraAutorizacion()),
                        credentials: 'omit'
                    });
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'No se pudo eliminar el comentario.'));
                    }
                    this.mostrarAlerta('success', 'Comentario eliminado.');
                    await this.cargarComentarios(this.paginaActual);
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                } finally {
                    this.cargando = false;
                }
            },

            async alternarLike(comentario) {
                const darLike = !this.leGusta(comentario);
                try {
                    const respuesta = await fetch(`/api/likes/comentarios/${comentario.idcomentario}`, {
                        method: darLike ? 'POST' : 'DELETE',
                        headers: Object.assign({ 'Content-Type': 'application/json' }, await this.cabeceraAutorizacion()),
                        credentials: 'omit'
                    });
                    if (!respuesta.ok) {
                        throw new Error(await this.errorDeRespuesta(respuesta, 'No se pudo actualizar el Me gusta.'));
                    }
                    await this.cargarComentarios(this.paginaActual);
                } catch (error) {
                    console.error('ComentariosVue:', error);
                    this.mostrarAlerta('danger', error.message);
                }
            },

            buscarComentario(id) {
                let encontrado = this.comentarios.find(c => c.idcomentario === id);
                if (!encontrado) {
                    for (const comentario of this.comentarios) {
                        if (!comentario.respuestas) continue;
                        const respuesta = comentario.respuestas.find(r => r.idcomentario === id);
                        if (respuesta) return respuesta;
                    }
                }
                return encontrado;
            }
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        const contenedor = document.getElementById('app-comentarios');
        if (!contenedor) return;
        if (typeof Vue === 'undefined') {
            console.error('ComentariosVue: Vue.js no está cargado.');
            return;
        }
        if (contenedor.getAttribute('data-montado') === 'true') return;
        contenedor.setAttribute('data-montado', 'true');

        Vue.createApp(componente).mount(contenedor);
    });
})();
