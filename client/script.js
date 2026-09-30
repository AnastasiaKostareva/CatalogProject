const API_URL = import.meta.env.VITE_API_URL;
const PLACEHOLDER_IMAGE = import.meta.env.VITE_PLACEHOLDER_IMAGE;

const grid       = document.getElementById('books-grid');
const modalEl    = document.getElementById('bookModal');
const modal      = new bootstrap.Modal(modalEl);
const form       = document.getElementById('book-form');
const modalTitle = document.getElementById('modal-title');
const formError  = document.getElementById('form-error');
const saveBtn    = document.getElementById('save-btn');

async function loadBooks() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error(`Ошибка ${response.status}`);

        const books = await response.json();
        grid.innerHTML = '';

        if (!books || books.length === 0) {
            grid.innerHTML = '<p class="empty-message">📚 Каталог пуст</p>';
            return;
        }

        books.forEach(book => {
            grid.insertAdjacentHTML('beforeend', createBookCard(book));
        });

    } catch (error) {
        console.error(error);
        grid.innerHTML = `<div class="empty-message">⚠️ ${error.message}</div>`;
    }
}

function createBookCard(book) {
    const year     = book.releaseYear ?? '—';
    const author   = book.author ?? 'Неизвестный автор';
    const genre    = book.genre ?? '';
    const language = book.language ?? '';
    const stars    = renderStars(book.rating);
    const poster   = book.imageUrl || PLACEHOLDER_IMAGE;

    const summary = book.summary
        ? (book.summary.length > 120 ? book.summary.slice(0, 120) + '…' : book.summary)
        : '';

    const metaParts = [year, genre, language].filter(Boolean);
    const metaLine  = metaParts.join(' · ');

    const badge = book.inProcess
        ? '<span class="book-card__badge">📖 Читаю</span>'
        : '';

    return `
        <article class="book-card" data-id="${book.id}">
            ${badge}
            <img src="${poster}"
                 alt="Обложка «${escapeHtml(book.title)}»"
                 class="book-card__poster"
                 onerror="this.src='${PLACEHOLDER_IMAGE}'">
            <div class="book-card__body">
                <h3 class="book-card__title" title="${escapeHtml(book.title)}">
                    ${escapeHtml(book.title) || 'Без названия'}
                </h3>
                <p class="book-card__meta">${escapeHtml(author)}</p>
                <p class="book-card__meta">${escapeHtml(metaLine)}</p>
                ${summary ? `<p class="book-card__summary">${escapeHtml(summary)}</p>` : ''}
                <div class="book-card__rating" title="Рейтинг: ${book.rating} из 5">
                    ${stars}
                </div>
                <div class="book-card__actions">
                    <button class="btn btn-sm btn-outline-secondary"
                            onclick="openEditModal(${book.id})">✏️ Изменить</button>
                    <button class="btn btn-sm btn-outline-danger"
                            onclick="deleteBook(${book.id})">🗑 Удалить</button>
                </div>
            </div>
        </article>
    `;
}

function renderStars(rating) {
    const safe = Math.max(0, Math.min(5, Number(rating) || 0));
    let html = '';
    for (let i = 1; i <= 5; i++) {
        html += i <= safe
            ? '<span class="star-filled">★</span>'
            : '<span class="star-empty">☆</span>';
    }
    return html;
}

function escapeHtml(text) {
    if (text == null) return '';
    return String(text)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

document.getElementById('add-book-btn').addEventListener('click', () => {
    form.reset();
    document.getElementById('book-id').value = '';
    document.getElementById('book-rating').value = 5;
    document.getElementById('book-in-process').checked = false;
    modalTitle.textContent = 'Добавить книгу';
    clearValidation();
    formError.classList.add('d-none');
    modal.show();
});

async function openEditModal(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`);
        if (!response.ok) throw new Error('Не удалось загрузить книгу');

        const book = await response.json();

        document.getElementById('book-id').value           = book.id;
        document.getElementById('book-title').value        = book.title ?? '';
        document.getElementById('book-author').value       = book.author ?? '';
        document.getElementById('book-year').value         = book.releaseYear ?? '';
        document.getElementById('book-genre').value        = book.genre ?? '';
        document.getElementById('book-language').value     = book.language ?? '';
        document.getElementById('book-rating').value       = book.rating ?? 5;
        document.getElementById('book-image').value        = book.imageUrl ?? '';
        document.getElementById('book-summary').value      = book.summary ?? '';
        document.getElementById('book-in-process').checked = !!book.inProcess;

        modalTitle.textContent = 'Редактировать книгу';
        clearValidation();
        formError.classList.add('d-none');
        modal.show();

    } catch (error) {
        alert(error.message);
    }
}
window.openEditModal = openEditModal;

form.addEventListener('submit', async (e) => {
    e.preventDefault();

    if (!validateForm()) return;

    const id = document.getElementById('book-id').value;

    const imageValue   = document.getElementById('book-image').value.trim();
    const genreValue   = document.getElementById('book-genre').value.trim();
    const summaryValue = document.getElementById('book-summary').value.trim();
    const yearValue    = document.getElementById('book-year').value;

    const book = {
        id:          id ? Number(id) : 0,
        title:       document.getElementById('book-title').value.trim(),
        author:      document.getElementById('book-author').value.trim(),
        language:    document.getElementById('book-language').value.trim(),
        rating:      Number(document.getElementById('book-rating').value),
        releaseYear: yearValue ? Number(yearValue) : null,
        genre:       genreValue || null,
        imageUrl:    imageValue || null,
        summary:     summaryValue || null,
        inProcess:   document.getElementById('book-in-process').checked
    };

    const isEdit = Boolean(id);
    const url    = isEdit ? `${API_URL}/${id}` : API_URL;
    const method = isEdit ? 'PUT' : 'POST';

    saveBtn.disabled = true;
    saveBtn.textContent = 'Сохранение...';

    try {
        const response = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(book)
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => null);
            throw new Error(formatServerError(errorData) || `Ошибка ${response.status}`);
        }

        modal.hide();
        await loadBooks();

    } catch (error) {
        formError.textContent = error.message;
        formError.classList.remove('d-none');
    } finally {
        saveBtn.disabled = false;
        saveBtn.textContent = 'Сохранить';
    }
});

async function deleteBook(id) {
    if (!confirm('Вы уверены, что хотите удалить книгу?')) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (!response.ok) throw new Error(`Ошибка ${response.status}`);
        await loadBooks();
    } catch (error) {
        alert('Не удалось удалить: ' + error.message);
    }
}
window.deleteBook = deleteBook;

function validateForm() {
    clearValidation();
    let isValid = true;

    const title    = document.getElementById('book-title');
    const author   = document.getElementById('book-author');
    const language = document.getElementById('book-language');
    const year     = document.getElementById('book-year');
    const rating   = document.getElementById('book-rating');
    const image    = document.getElementById('book-image');

    if (!title.value.trim())    { markInvalid(title);    isValid = false; }
    if (!author.value.trim())   { markInvalid(author);   isValid = false; }
    if (!language.value.trim()) { markInvalid(language); isValid = false; }

    if (year.value) {
        const y = Number(year.value);
        if (y < 1 || y > 2026) { markInvalid(year); isValid = false; }
    }

    const r = Number(rating.value);
    if (r < 1 || r > 5 || !Number.isInteger(r)) {
        markInvalid(rating);
        isValid = false;
    }

    if (image.value.trim() && !isValidUrl(image.value.trim())) {
        markInvalid(image);
        isValid = false;
    }

    return isValid;
}

function markInvalid(input) {
    input.classList.add('is-invalid');
    input.classList.remove('is-valid');
}

function clearValidation() {
    form.querySelectorAll('.is-invalid, .is-valid').forEach(el => {
        el.classList.remove('is-invalid', 'is-valid');
    });
}

function isValidUrl(str) {
    try { new URL(str); return true; } catch { return false; }
}

function formatServerError(errorData) {
    if (!errorData) return null;

    if (errorData.errors) {
        const messages = [];
        for (const field in errorData.errors) {
            messages.push(...errorData.errors[field]);
        }
        return messages.join('\n');
    }
    return errorData.title || errorData.detail || errorData.message || null;
}

document.addEventListener('DOMContentLoaded', loadBooks);