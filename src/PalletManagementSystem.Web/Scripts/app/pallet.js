/* Common Styles for Pallet Management System
 * Designed for IE11 compatibility
 */

/* General Styles */
body {
    font - family: 'Segoe UI', sans - serif;
}

.navbar {
    background - color: #003366;
}

.navbar - brand {
    font - weight: bold;
}

/* Card Styles */
.card {
    border - radius: 8px;
    -webkit - box - shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    box - shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    margin - bottom: 20px;
}

.card - header {
    background - color: #e6eef5;
    color: #003366;
    font - weight: bold;
}

/* Button Styles */
.btn - primary {
    background - color: #003366;
    border - color: #003366;
}

.btn - primary:hover {
    background - color: #5e87b0;
    border - color: #5e87b0;
}

/* Badge Styles */
.badge - success {
    background - color: #2e7d32;
}

.badge - orange {
    background - color: #f57c00;
    color: white;
}

/* Header Styles */
.pallet - header, .create - header, .edit - header {
    background - color: #e6eef5;
    border - radius: 8px;
    padding: 20px;
    margin - bottom: 20px;
}

.pallet - header h4, .create - header h4, .edit - header h4 {
    color: #003366;
    margin - bottom: 0;
}

/* Table Styles */
.table th {
    background - color: #e6eef5;
    color: #003366;
}

/* Search Styles */
.search - container {
    position: relative;
}

.search - results {
    position: absolute;
    top: 100 %;
    left: 0;
    right: 0;
    background: white;
    border: 1px solid #ddd;
    border - radius: 0 0 4px 4px;
    z - index: 1000;
    display: none;
}

.search - results.show {
    display: block;
}

.search - item {
    padding: 8px 12px;
    border - bottom: 1px solid #eee;
    cursor: pointer;
}

.search - item:hover {
    background - color: #e6eef5;
}

/* Info Item Styles */
.pallet - info - item {
    display: table;
    width: 100 %;
    margin - bottom: 10px;
}

.pallet - info - label {
    display: table - cell;
    min - width: 150px;
    font - weight: 600;
    color: #003366;
}

.pallet - info - value {
    display: table - cell;
}

/* Activity Log Styles */
.activity - item {
    margin - bottom: 15px;
    position: relative;
    padding - left: 40px;
}

.activity - icon {
    position: absolute;
    left: 0;
    top: 0;
}

.activity - content {
    padding - left: 10px;
}

.activity - title {
    font - weight: 600;
}

.activity - subtitle {
    color: #555;
}

/* Touch Mode Styles */
.touch - input {
    height: 50px;
    font - size: 1.1rem;
}

.num - keypad {
    display: -ms - grid;
    display: grid;
    -ms - grid - columns: 1fr 10px 1fr 10px 1fr;
    grid - template - columns: repeat(3, 1fr);
    grid - gap: 10px;
    max - width: 300px;
    margin - top: 10px;
}

.num - key {
    width: 100 %;
    height: 50px;
    font - size: 1.2rem;
    background - color: #f8f9fa;
}

/* IE11 Grid fallback for num-keypad */
@media all and(-ms - high - contrast: none), (-ms - high - contrast: active) {
    .num - keypad {
        display: block;
        overflow: hidden;
    }
    
    .num - key {
        float: left;
        width: 30 %;
        margin: 1.66 %;
    }
}

/* Validation Styles */
.is - invalid {
    border - color: #c62828!important;
}

.text - danger {
    color: #c62828;
    font - size: 0.875rem;
}

/* Modal Styles */
.modal - header {
    background - color: #003366;
    color: white;
}

.modal - footer {
    background - color: #e6eef5;
}

/* IE11 Flexbox fixes */
.d - flex {
    display: -ms - flexbox;
    display: flex;
}

.align - items - center {
    -ms - flex - align: center;
    align - items: center;
}

.justify - content - between {
    -ms - flex - pack: justify;
    justify - content: space - between;
}

.text - md - right {
    text - align: right;
}

.mt - md - 0 {
    margin - top: 0;
}