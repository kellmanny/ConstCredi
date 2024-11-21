document.addEventListener("DOMContentLoaded", () => {
  const userId = getUserIdFromToken();

  if (!userId) {
    alert("Usuário não encontrado! Faça login novamente.");
    localStorage.removeItem("token");
    window.location.href = "login.html";
    return;
  }

  loadUserProfile(userId);
  loadUserPosts(userId);
});

// Função para obter o userId do token armazenado no localStorage
function getUserIdFromToken() {
  const token = localStorage.getItem("token");
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1])); // Decodifica o payload do JWT
    return payload.nameid || null; // Certifique-se de usar o campo correto no payload (ex.: nameid)
  } catch (error) {
    console.error("Erro ao decodificar o token:", error);
    return null;
  }
}

// Função para carregar o perfil do usuário
async function loadUserProfile(userId) {
  try {
    const response = await fetch(`${API_URL}/User/${userId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const userData = await response.json();
      displayUserProfile(userData);
    } else {
      console.error("Erro ao carregar perfil:", response.statusText);
      alert("Erro ao carregar perfil do usuário.");
    }
  } catch (error) {
    console.error("Erro ao carregar perfil:", error);
    alert("Erro inesperado ao carregar perfil.");
  }
}

// Função para exibir o perfil do usuário
function displayUserProfile(userData) {
  document.getElementById("username").innerText =
    userData.username || "Usuário Desconhecido";

  // Exibe a biografia
  document.getElementById("email").innerText =
    userData.biography || "Biografia não disponível";

  document.getElementById("profilePicture").src =
    userData.profilePic
      ? `${API_URL.replace('/api', '')}${userData.profilePic}?t=${new Date().getTime()}`
      : "assets/profile-pic-placeholder.png";

  // Exibe a quantidade de seguidores e seguindo
  const followersAndFollowing = `
    <span>${userData.followers || 0} Seguidores</span> |
    <span>Seguindo ${userData.following || 0}</span>
  `;
  document.getElementById("followersAndFollowing").innerHTML =
    followersAndFollowing;
}
// Função para carregar os posts do usuário
async function loadUserPosts(userId) {
  try {
    const response = await fetch(`${API_URL}/Post/user/${userId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const posts = await response.json();
      displayUserPosts(posts);
    } else {
      console.error("Erro ao carregar posts:", response.statusText);
      alert("Erro ao carregar posts do usuário.");
    }
  } catch (error) {
    console.error("Erro ao carregar posts:", error);
    alert("Erro inesperado ao carregar posts.");
  }
}

// Função para exibir os posts do usuário no formato da timeline
function displayUserPosts(posts) {
  const postsContainer = document.getElementById("userPostsContainer");
  postsContainer.innerHTML = ""; // Limpa os posts anteriores

  if (posts.length === 0) {
    postsContainer.innerHTML = "<p>Nenhum post encontrado.</p>";
    return;
  }

  posts.forEach((post) => {
    const postElement = document.createElement("div");
    postElement.classList.add("post");

    postElement.innerHTML = `
      <div class="post-header">
        <img src="${post.profilePic ? `${API_URL.replace('/api', '')}${post.profilePic}?t=${new Date().getTime()}` : 'assets/profile-pic-placeholder.png'}" 
            alt="Foto de perfil" class="profile-pic">
        <div class="user-info">
          <span class="username">${post.username || "Usuário Desconhecido"}</span>
          <span class="user-id">@${post.userId || "undefined"}</span>
          <br>
          <span class="timestamp">${new Date(post.createdAt).toLocaleString()}</span>
        </div>
      </div>
      <div class="post-content">${post.content || "Sem conteúdo disponível"}</div>
      <div class="post-footer">
        <button class="icon-button" onclick="toggleComments(${post.id})">
          <img src="assets/botaoComentar.png" alt="Comentar" class="icon">
          <span>${post.comments?.length || 0}</span>
        </button>
        <button class="icon-button" onclick="likePost(${post.id})">
          <img src="assets/botaoAmei.png" alt="Curtir" class="icon">
          <span id="likes-count-${post.id}">${post.likes || 0}</span>
        </button>
      </div>
      <div id="comments-${post.id}" class="comments-section" style="display: none;">
        ${renderComments(post.comments, post.id)}
        <textarea 
          id="new-comment-${post.id}" 
          placeholder="Escreva um comentário..." 
          class="comment-box">
        </textarea>
        <button 
          onclick="addComment(${post.id})"
          class="comment-post-button">
          Postar
        </button>
      </div>
    `;

    postsContainer.appendChild(postElement);
  });
}

// Função para renderizar comentários existentes
function renderComments(comments = [], postId) {
  if (!comments || comments.length === 0) {
    return "<p>Sem comentários ainda.</p>";
  }

  return comments
    .map((comment) => {
      return `
      <div class="comment">
        <div class="comment-header">
          <img src="${
            comment.profilePic
              ? `${API_URL.replace('/api', '')}${comment.profilePic}?t=${new Date().getTime()}`
              : "assets/profile-pic-placeholder.png"
          }" alt="Foto de Perfil" class="profile-pic">
          <div class="user-info">
            <span class="username">${comment.username || "Usuário Desconhecido"}</span>
            <span class="user-id">@${comment.userId || "undefined"}</span>
            <br>
            <span class="timestamp">${new Date(comment.createdAt).toLocaleString()}</span>
          </div>
        </div>
        <div class="comment-content">${comment.content || "Sem conteúdo disponível"}</div>
        <div class="comment-footer">
          <button class="icon-button" onclick="replyToComment(${comment.userId}, ${postId})">
            <img src="assets/botaoComentar.png" alt="Responder" class="icon">
          </button>
          <button class="icon-button" onclick="likeComment(${comment.id})">
            <img src="assets/botaoAmei.png" alt="Curtir" class="icon">
            <span id="likes-count-comment-${comment.id}">${comment.likes || 0}</span>
          </button>
        </div>
      </div>`;
    })
    .join("");
}

// Função para alternar a exibição de comentários
function toggleComments(postId) {
  const commentsSection = document.getElementById(`comments-${postId}`);
  if (commentsSection) {
    commentsSection.style.display =
      commentsSection.style.display === "none" ? "block" : "none";
  }
}

// Função para curtir um post
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
      console.error("Erro ao curtir post:", response.statusText);
    }
  } catch (error) {
    console.error("Erro ao curtir post:", error);
  }
}

// Função para adicionar um comentário
async function addComment(postId) {
  const commentContent = document.getElementById(`new-comment-${postId}`).value;

  if (!commentContent.trim()) {
    alert("Por favor, escreva algo no comentário!");
    return;
  }

  const payload = {
    postId: postId,
    type: "comment",
    content: commentContent,
  };

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
      document.getElementById(`new-comment-${postId}`).value = ""; // Limpar campo de texto
      loadUserPosts(getUserIdFromToken()); // Recarrega os posts
    } else {
      console.error("Erro ao adicionar comentário:", await response.json());
    }
  } catch (error) {
    console.error("Erro ao adicionar comentário:", error);
  }
}

// Função para curtir um comentário
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
      console.error("Erro ao curtir comentário:", response.statusText);
    }
  } catch (error) {
    console.error("Erro ao curtir comentário:", error);
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

// Redirecionar para a página de edição de perfil
function redirectToEditProfile() {
  window.open("editar-perfil.html", "_blank"); // Abre em uma nova aba
}
const voltarATimeline = document.getElementById("voltarATimeline");
if (voltarATimeline) {
  voltarATimeline.addEventListener("click", () => {
    window.location.href = "index.html";
  });
}




