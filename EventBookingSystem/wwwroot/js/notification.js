"use strict";

const notificationPagingState = {
    pageSize: 10,
    loadedNotificationCount: 0,
    isEnd: false,
    isLoading: false
};

//get notifications from the server
document.addEventListener("DOMContentLoaded", () => {
    const dropdown = document.getElementById("notification-dropdown");
    if (!dropdown) {
        return;
    }

    const notificationUrl = dropdown.dataset.notificationUrl;
    const list = document.getElementById("notification-list");
    const unreadBadge = document.getElementById("notification-unread-count");
    const unreadBadgeValue = document.getElementById("notification-unread-count-value");

    if (!notificationUrl || !list || !unreadBadge || !unreadBadgeValue) {
        return;
    }

    loadNotifications(notificationUrl, list, unreadBadge, unreadBadgeValue, notificationPagingState);

    list.addEventListener("click", event => {
        if (!(event.target instanceof Element)) {
            return;
        }

        const loadMoreButton = event.target.closest(".notification-load-more button");

        if (!loadMoreButton) {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        loadNotifications(notificationUrl, list, unreadBadge, unreadBadgeValue, notificationPagingState, true);
    });
});

async function loadNotifications(notificationUrl, list, unreadBadge, unreadBadgeValue, state, append = false) {
    if (state.isLoading || (append && state.isEnd)) {
        return;
    }

    state.isLoading = true;

    try {
        const url = new URL(notificationUrl, window.location.origin);
        url.searchParams.set("skip", append ? state.loadedNotificationCount : 0);
        url.searchParams.set("take", state.pageSize);

        const response = await fetch(url, {
            headers: {
                "Accept": "application/json"
            }
        });

        if (!response.ok) {
            throw new Error("Failed to load notifications.");
        }

        const page = normalizeNotificationPage(await response.json());
        renderNotifications(page, list, unreadBadge, unreadBadgeValue, state, append);
    } catch {
        if (!append) {
            list.replaceChildren(createNotificationState("Could not load notifications."));
            updateUnreadBadge(unreadBadge, unreadBadgeValue, 0);
        }
    } finally {
        state.isLoading = false;
    }
}

function renderNotifications(page, list, unreadBadge, unreadBadgeValue, state, append) {
    const notifications = page.notifications;

    removeLoadMoreButton(list);

    if (!append) {
        state.loadedNotificationCount = 0;
        list.replaceChildren();
    }

    if (!append && notifications.length === 0) {
        list.appendChild(createNotificationState("No notifications yet."));
        updateUnreadBadge(unreadBadge, unreadBadgeValue, 0);
        return;
    }

    notifications.forEach(notification => {
        list.appendChild(createNotificationItem(notification));
    });

    state.loadedNotificationCount += notifications.length;
    state.isEnd = page.isEnd;

    if (!state.isEnd) {
        list.appendChild(createLoadMoreButton());
    }

    updateUnreadBadgeFromList(list, unreadBadge, unreadBadgeValue);
}

//helper methods
function createNotificationItem(notification) {
    const title = getNotificationValue(notification, "title") || "Notification";
    const message = getNotificationValue(notification, "message") || "";
    const isRead = getNotificationValue(notification, "isRead");
    const createdDate = getNotificationValue(notification, "createdDate");

    const item = document.createElement("div");
    item.className = `list-group-item flex-column align-items-start notification-item${isRead ? "" : " active notification-item-unread"}`;

    const headerElement = document.createElement("div");
    headerElement.className = "d-flex w-100 justify-content-between gap-3";

    const titleElement = document.createElement("h6");
    titleElement.className = "mb-1 notification-title";
    titleElement.textContent = title;

    const dateElement = document.createElement("small");
    dateElement.className = `notification-date flex-shrink-0${isRead ? " text-muted" : ""}`;
    dateElement.textContent = formatNotificationDate(createdDate);

    const messageElement = document.createElement("div");
    messageElement.className = `mb-1 notification-message${isRead ? " text-muted" : ""}`;
    messageElement.textContent = message;

    headerElement.append(titleElement, dateElement);
    item.append(headerElement, messageElement);
    return item;
}

function createNotificationState(message) {
    const state = document.createElement("div");
    state.className = "list-group-item text-muted small notification-state";
    state.textContent = message;
    return state;
}

function createLoadMoreButton() {
    const wrapper = document.createElement("div");
    wrapper.className = "list-group-item notification-load-more p-2";

    const button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-outline-primary btn-sm w-100";
    button.textContent = "Load more";

    wrapper.appendChild(button);
    return wrapper;
}

function removeLoadMoreButton(list) {
    list.querySelector(".notification-load-more")?.remove();
}

function updateUnreadBadge(unreadBadge, unreadBadgeValue, unreadCount) {
    unreadBadgeValue.textContent = unreadCount;
    unreadBadge.classList.toggle("d-none", unreadCount === 0);
}

function updateUnreadBadgeFromList(list, unreadBadge, unreadBadgeValue) {
    updateUnreadBadge(unreadBadge, unreadBadgeValue, list.querySelectorAll(".notification-item-unread").length);
}

function getNotificationValue(notification, camelCaseName) {
    if (!notification) {
        return undefined;
    }

    const pascalCaseName = camelCaseName.charAt(0).toUpperCase() + camelCaseName.slice(1);
    return notification[camelCaseName] ?? notification[pascalCaseName];
}

function normalizeNotificationPage(value) {
    if (Array.isArray(value)) {
        return {
            notifications: value,
            isEnd: true
        };
    }

    const notifications = getNotificationValue(value, "notifications");
    const isEnd = getNotificationValue(value, "isEnd");

    return {
        notifications: Array.isArray(notifications) ? notifications : [],
        isEnd: Boolean(isEnd)
    };
}

function formatNotificationDate(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return "";
    }

    return date.toLocaleString(undefined, {
        month: "short",
        day: "numeric",
        hour: "numeric",
        minute: "2-digit"
    });
}

function clearUnreadBadge() {
    const unreadBadge = document.getElementById("notification-unread-count");
    const unreadBadgeValue = document.getElementById("notification-unread-count-value");

    if (!unreadBadge || !unreadBadgeValue) {
        return;
    }

    updateUnreadBadge(unreadBadge, unreadBadgeValue, 0);
}

function markNotificationsRead() {
    document.querySelectorAll(".notification-item-unread")
        .forEach(item => {
            item.classList.remove("active", "notification-item-unread");
            item.querySelector(".notification-date")?.classList.add("text-muted");
            item.querySelector(".notification-message")?.classList.add("text-muted");
        });

    clearUnreadBadge();
}

function removeNotificationStates(list) {
    list.querySelectorAll(".notification-state").forEach(state => state.remove());
}

//update status
let isUpdatingNotifications = false;

async function updateStatus() {

    if (isUpdatingNotifications)
        return;

    isUpdatingNotifications = true;

    const dropdown = document.getElementById("notification-dropdown");

    if (!dropdown) {
        isUpdatingNotifications = false;
        return;
    }

    const url = dropdown.dataset.apiStatusUrl;

    if (!url) {
        isUpdatingNotifications = false;
        return;
    }

    const unreadBadge = document.getElementById("notification-unread-count");
    const unreadBadgeValue = document.getElementById("notification-unread-count-value");

    if (!unreadBadge || !unreadBadgeValue) {
        isUpdatingNotifications = false;
        return;
    }

    try {

        const response = await fetch(url, { method: "PATCH" });

        if (!response.ok) {
            throw new Error("Failed to update notification status.");
        }

        markNotificationsRead();
        console.log("Server succeeded");

    } catch (err) {
        console.error(err);

    } finally {
        isUpdatingNotifications = false;
    }
}

const dropdown = document.getElementById("notification-dropdown");

if (dropdown) {

    dropdown.addEventListener("hidden.bs.dropdown", () => {

        const unreadBadgeValue =
            document.getElementById("notification-unread-count-value");

        if (unreadBadgeValue && Number(unreadBadgeValue.textContent) > 0)
            updateStatus();
    });
}

//signalr connection
const signalRConnection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

// register notify method to add new notification when needed
signalRConnection.on("Notify", (notification) => {
    console.log(notification);
    const list = document.getElementById("notification-list");
    const unreadBadge = document.getElementById("notification-unread-count");
    const unreadBadgeValue = document.getElementById("notification-unread-count-value");

    if (!list || !unreadBadge || !unreadBadgeValue) {
        return;
    }

    removeNotificationStates(list);

    list.prepend(createNotificationItem(notification));
    notificationPagingState.loadedNotificationCount += 1;
    unreadBadgeValue.textContent = Number(unreadBadgeValue.textContent ?? 0) + 1;
    unreadBadge.classList.remove("d-none");
});

// register updateReadStatus for multiple taps sync
signalRConnection.on("UpdateReadStatus", () => {
    markNotificationsRead();
});

async function start() {
    try {
        await signalRConnection.start();
        console.log("SignalR Connected.");
    }
    catch (err) {
        console.error(err);
        setTimeout(start, 5000);
    }
}

signalRConnection.onclose(async () => {
    await start();
});

start();