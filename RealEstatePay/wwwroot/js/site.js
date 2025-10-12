// Dashboard Modal Functions
function showConfirmModal() {
    const form = document.getElementById('paymentForm');
    const formData = new FormData(form);
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    let details = '';
    details += `<div class="detail-row"><span class="detail-label">Customer Name:</span><span>${formData.get('CustomerName') || 'N/A'}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Plot Number:</span><span>${formData.get('PlotNumber') || 'N/A'}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Site/Layout:</span><span>${formData.get('SiteOrLayoutName')}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Layout Number:</span><span>${formData.get('LayoutNumber') || 'N/A'}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Contact Number:</span><span>${formData.get('ContactNumber')}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Message:</span><span>${formData.get('Message') || 'N/A'}</span></div>`;
    details += `<div class="detail-row"><span class="detail-label">Amount Paid:</span><span>₹${formData.get('AmountPaid')}</span></div>`;
    
    document.getElementById('modalDetails').innerHTML = details;
    document.getElementById('confirmModal').style.display = 'block';
}

function closeModal() {
    document.getElementById('confirmModal').style.display = 'none';
}

function sendSMS() {
    const form = document.getElementById('paymentForm');
    const formData = new FormData(form);
    
    const hiddenForm = document.createElement('form');
    hiddenForm.method = 'POST';
    hiddenForm.action = window.dashboardUrl || '/PaymentSms/Dashboard';
    
    for (let [key, value] of formData.entries()) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = key;
        input.value = value;
        hiddenForm.appendChild(input);
    }
    
    document.body.appendChild(hiddenForm);
    hiddenForm.submit();
}

// Contacts Modal Functions
function showDetails(text) {
    document.getElementById('modalText').innerText = text;
    document.getElementById('detailsModal').style.display = 'block';
}

function closeDetails() {
    document.getElementById('detailsModal').style.display = 'none';
}

// Global Modal Click Handler
window.onclick = function(event) {
    const confirmModal = document.getElementById('confirmModal');
    const detailsModal = document.getElementById('detailsModal');
    
    if (event.target == confirmModal) {
        closeModal();
    }
    
    if (event.target == detailsModal) {
        closeDetails();
    }
}