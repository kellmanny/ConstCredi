document.addEventListener("DOMContentLoaded", () => {
  const notificationsButton = document.getElementById("notificationsButton");
  const notificationsContainer = document.getElementById("notificationsContainer");
  const notificationsList = document.getElementById("notificationsList");

  // Abrir/fechar notificações ao clicar no botão
  notificationsButton.addEventListener("click", () => {
    const isVisible = notificationsContainer.style.display === "block";
    notificationsContainer.style.display = isVisible ? "none" : "block";

    if (!isVisible) {
      loadNotifications(); // Carrega notificações ao abrir
    }
  });
});

// Função para carregar notificações
async function loadNotifications() {
  const notificationsList = document.getElementById("notificationsList");
  notificationsList.innerHTML = "<li>Carregando...</li>";

  try {
    const response = await fetch(`${API_URL}/Notification`, {
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      const notifications = await response.json();
      renderNotifications(notifications);
    } else {
      notificationsList.innerHTML = "<li>Erro ao carregar notificações.</li>";
    }
  } catch (error) {
    console.error("Erro ao carregar notificações:", error);
    notificationsList.innerHTML = "<li>Erro inesperado ao carregar notificações.</li>";
  }
}

// Função para renderizar notificações
function renderNotifications(notifications) {
  const notificationsList = document.getElementById("notificationsList");
  notificationsList.innerHTML = "";

  if (notifications.length === 0) {
    notificationsList.innerHTML = "<li>Sem notificações.</li>";
    return;
  }

  notifications.forEach((notification) => {
    const notificationItem = document.createElement("li");
    notificationItem.className = `notification-item ${notification.isRead ? "" : "unread"}`;
    notificationItem.innerHTML = `
      <div class="notification-message">${notification.message}</div>
      <div class="notification-time">${new Date(notification.createdAt).toLocaleString()}</div>
      <button class="mark-as-read-button" onclick="markNotificationAsRead(${notification.id})">
        Marcar como lida
      </button>
    `;

    notificationsList.appendChild(notificationItem);
  });
}

// Função para marcar uma notificação como lida
async function markNotificationAsRead(notificationId) {
  try {
    const response = await fetch(`${API_URL}/Notification/${notificationId}/mark-as-read`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      loadNotifications(); // Recarrega notificações após marcar como lida
    } else {
      alert("Erro ao marcar notificação como lida.");
    }
  } catch (error) {
    console.error("Erro ao marcar notificação como lida:", error);
  }
}

// Função para marcar todas as notificações como lidas
async function markAllNotificationsAsRead() {
  try {
    const response = await fetch(`${API_URL}/Notification/mark-all-as-read`, {
      method: "PUT",
      headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
    });

    if (response.ok) {
      loadNotifications(); // Recarrega notificações após marcar todas como lidas
    } else {
      alert("Erro ao marcar todas as notificações como lidas.");
    }
  } catch (error) {
    console.error("Erro ao marcar todas as notificações como lidas:", error);
  }
}

// Função para criar uma notificação ao curtir um post
async function createLikeNotification(postId) {
  try {
    const response = await fetch(`${API_URL}/Notification/like-notification`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
      body: JSON.stringify({ postId }),
    });

    if (!response.ok) {
      console.error("Erro ao criar notificação de curtida:", await response.json());
    }
  } catch (error) {
    console.error("Erro ao criar notificação de curtida:", error);
  }
}

// Função para criar uma notificação ao comentar em um post
async function createCommentNotification(postId, commentContent) {
  try {
    const response = await fetch(`${API_URL}/Notification/comment-notification`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
      body: JSON.stringify({ postId, commentContent }),
    });

    if (!response.ok) {
      console.error("Erro ao criar notificação de comentário:", await response.json());
    }
  } catch (error) {
    console.error("Erro ao criar notificação de comentário:", error);
  }
}
