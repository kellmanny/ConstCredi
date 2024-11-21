// Estado de visibilidade dos comentários
const commentsVisibility = {};

// Quando a página for carregada
document.addEventListener("DOMContentLoaded", () => {
  if (!localStorage.getItem("token")) {
    alert("Você não está autenticado! Faça login novamente.");
    window.location.href = "login.html"; // Redirecionar para a página de login
    return;
  }

  // Configurar botões de perfil e logout
  const profileButton = document.getElementById("profileButton");
  const logoutButton = document.getElementById("logoutButton");

  if (profileButton) {
    profileButton.addEventListener("click", () => {
      window.location.href = "perfil.html";
    });
  }

  if (logoutButton) {
    logoutButton.addEventListener("click", () => {
      localStorage.removeItem("token");
      window.location.href = "login.html";
    });
  }

  loadPosts();

  const createPostButton = document.getElementById("createPostButton");
  if (createPostButton) {
    createPostButton.addEventListener("click", createPost);
  }

  // Atualizar posts automaticamente a cada 10 segundos
  setInterval(() => {
    loadPosts(true);
  }, 10000);
});

// Função para criar um novo post
async function createPost() {
  const postContent = document.getElementById("postContent").value;

  if (!postContent.trim()) {
    alert("Por favor, insira algum conteúdo!");
    return;
  }

  const postData = { content: postContent, mediaUrl: "" };

  try {
    const response = await fetch(`${API_URL}/Post/create`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
      body: JSON.stringify(postData),
    });

    if (response.ok) {
      alert("Post criado com sucesso!");
      document.getElementById("postContent").value = "";
      loadPosts();
    } else {
      const error = await response.json();
      alert(error.message || "Erro ao criar o post");
    }
  } catch (error) {
    console.error("Erro ao criar o post:", error);
    alert("Erro inesperado ao criar o post.");
  }
}

// Função para carregar posts
async function loadPosts(preserveVisibility = false) {
  try {
    const response = await fetch(`${API_URL}/Post/timeline-with-interactions`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const posts = await response.json();
      displayPosts(posts, preserveVisibility);
    } else if (response.status === 401) {
      alert("Sessão expirada. Faça login novamente.");
      localStorage.removeItem("token");
      window.location.href = "login.html";
    } else {
      alert("Erro ao carregar posts!");
    }
  } catch (error) {
    console.error("Erro ao carregar os posts:", error);
    alert("Erro inesperado ao carregar os posts!");
  }
}

// Função para exibir posts
function displayPosts(posts, preserveVisibility = false) {
  const postsContainer = document.getElementById("postsContainer");
  postsContainer.innerHTML = "";

  posts.forEach((post) => {
    const postId = post.id || "ID desconhecido";
    const username = post.username || "Usuário desconhecido";
    const userId = post.userId || "ID não disponível";
    const content = post.content || "Conteúdo não disponível";
    const createdAt = post.createdAt
      ? new Date(post.createdAt).toLocaleString()
      : "Data desconhecida";
    const likes = post.likes || 0;
    const profilePic = post.profilePic
      ? `${API_URL.replace('/api', '')}${post.profilePic}?t=${new Date().getTime()}`
      : "assets/profile-pic-placeholder.png";

    const postElement = document.createElement("div");
    postElement.classList.add("post");

    postElement.innerHTML = `
      <div class="post-header">
        <img src="${profilePic}" alt="Foto de perfil" class="profile-pic">
        <div class="user-info">
          <a href="perfil.html?userId=${userId}" class="username">
            ${username} <span class="user-id">@${userId}</span>
          </a>
          <span class="timestamp">${createdAt}</span>
        </div>
      </div>
      <div class="post-content">${content}</div>
      <div class="post-footer">
        <button class="icon-button" onclick="toggleComments(${postId})">
          <img src="assets/botaoComentar.png" alt="Comentar">
          <span>${post.comments?.length || 0}</span>
        </button>
        <button class="icon-button" onclick="likePost(${postId})">
          <img src="assets/botaoAmei.png" alt="Curtir">
          <span id="likes-count-${postId}">${likes}</span>
        </button>
      </div>
      <div id="comments-${postId}" class="comments-section" style="display: none;">
        ${post.comments
          ?.map((comment) => displayComment(comment))
          .join("") || "<p>Sem comentários ainda.</p>"}
        <textarea 
          id="new-comment-${postId}" 
          placeholder="Escreva um comentário..."
          class="comment-box"
        ></textarea>
        <button 
          onclick="addComment(${postId})"
          class="comment-post-button">
          Postar
        </button>
      </div>
    `;

    postsContainer.appendChild(postElement);

    if (preserveVisibility && commentsVisibility[postId]) {
      toggleComments(postId, true);
    }
  });
}

// Função para exibir um comentário
function displayComment(comment) {
  const commentId = comment.id || `comment-${Math.random()}`;
  const username = comment.username || "Usuário desconhecido";
  const userId = comment.userId || "ID não disponível";
  const content = comment.content || "Conteúdo não disponível";
  const createdAt = comment.createdAt
    ? new Date(comment.createdAt).toLocaleString()
    : "Data desconhecida";
  const likes = comment.likes || 0;
  const profilePic = comment.profilePic
    ? `${API_URL.replace('/api', '')}${comment.profilePic}?t=${new Date().getTime()}`
    : "assets/profile-pic-placeholder.png";

  return `
    <div class="comment">
      <div class="comment-header">
        <img src="${profilePic}" alt="Foto de Perfil" class="profile-pic">
        <div class="user-info">
          <a href="perfil.html?userId=${userId}" class="username">
            ${username} <span class="user-id">@${userId}</span>
          </a>
          <span class="timestamp">${createdAt}</span>
        </div>
      </div>
      <div class="comment-content">${content}</div>
      <div class="comment-footer">
        <button class="icon-button" onclick="likeComment(${commentId})">
          <img src="assets/botaoAmei.png" alt="Curtir">
          <span id="likes-count-comment-${commentId}">${likes}</span>
        </button>
      </div>
    </div>
  `;
}

// Função para alternar comentários
function toggleComments(postId, forceOpen = false) {
  const commentsSection = document.getElementById(`comments-${postId}`);
  if (commentsSection) {
    const isVisible = commentsSection.style.display === "block";
    commentsSection.style.display = forceOpen || !isVisible ? "block" : "none";
    commentsVisibility[postId] = !isVisible;
  }
}

// Função para adicionar um comentário
async function addComment(postId) {
  const commentContent = document.getElementById(`new-comment-${postId}`).value;

  if (!commentContent.trim()) {
    alert("Por favor, escreva algo no comentário!");
    return;
  }

  const payload = { postId, type: "comment", content: commentContent };

  try {
    const response = await fetch(`${API_URL}/Interaction/interact`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      document.getElementById(`new-comment-${postId}`).value = "";
      loadPosts(true);
    } else {
      const error = await response.json();
      alert(error.message || "Erro ao adicionar comentário.");
    }
  } catch (error) {
    console.error("Erro ao adicionar comentário:", error);
    alert("Erro inesperado ao adicionar comentário.");
  }
}

// Funções de curtidas para posts e comentários
async function likePost(postId) {
  try {
    const response = await fetch(`${API_URL}/Interaction/like/${postId}`, {
      method: "POST",
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const data = await response.json();
      document.getElementById(`likes-count-${postId}`).innerText = data.likes;
    } else {
      const error = await response.json();
      alert(`Erro ao curtir o post: ${error.message || "Erro desconhecido"}`);
    }
  } catch (error) {
    console.error("Erro ao curtir o post:", error);
    alert("Erro inesperado ao curtir o post.");
  }
}

async function likeComment(commentId) {
  try {
    const response = await fetch(`${API_URL}/Interaction/like/${commentId}`, {
      method: "POST",
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const data = await response.json();
      document.getElementById(`likes-count-comment-${commentId}`).innerText =
        data.likes;
    } else {
      const error = await response.json();
      alert(`Erro ao curtir o comentário: ${error.message || "Erro desconhecido"}`);
    }
  } catch (error) {
    console.error("Erro ao curtir o comentário:", error);
    alert("Erro inesperado ao curtir o comentário.");
  }
}

// Função para responder a um comentário
function replyToComment(ownerUserId, postId) {
  const textarea = document.getElementById(`new-comment-${postId}`);
  if (textarea) {
    textarea.value = `@${ownerUserId} `;
    textarea.focus();
  } else {
    alert("Erro ao encontrar o campo de texto para resposta.");
  }
}

// Função auxiliar para obter o ID do usuário do token
function getUserIdFromToken() {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1])); // Decodifica o payload do JWT
    return payload.nameid || null; // Certifique-se de usar o campo correto do payload (ex.: nameid)
  } catch (error) {
    console.error("Erro ao decodificar o token:", error);
    return null;
  }
}
